using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using CefInterop;
using Hacknet;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using MonoMod.Cil;

namespace Pathfinder;

[HarmonyPatch]
public static unsafe partial class NewCefImpl
{
    [UnmanagedCallersOnly]
    static void AddRef(_cef_base_ref_counted_t* counter)
    {
    }

    [UnmanagedCallersOnly]
    static int Release(_cef_base_ref_counted_t* counter)
    {
        return 1;
    }

    [UnmanagedCallersOnly]
    static int HasOneRef(_cef_base_ref_counted_t* _)
    {
        return 1;
    }

    [UnmanagedCallersOnly]
    static int HasAtLeastOneRef(_cef_base_ref_counted_t* _)
    {
        return 1;
    }

    [UnmanagedCallersOnly]
    static void StringDealloc(ushort* str) => Utf16StringMarshaller.Free(str);

    [UnmanagedCallersOnly]
    static _cef_browser_process_handler_t* GetProcessHandler(_cef_app_t* _) => null;
    
    [UnmanagedCallersOnly]
    static _cef_resource_bundle_handler_t* GetResourceBundleHandler(_cef_app_t* _) => null;

    [UnmanagedCallersOnly]
    static _cef_render_process_handler_t* GetRenderProcessHandler(_cef_app_t* _) => null;

    [UnmanagedCallersOnly]
    static void OnBeforeCommandLineProcessing(_cef_app_t* app, _cef_string_utf16_t* something, _cef_command_line_t* commandLine) { }
    
    [UnmanagedCallersOnly]
    static void OnBeforeSchemaRegister(_cef_app_t* app, _cef_scheme_registrar_t* schemas) { }

    [UnmanagedCallersOnly]
    static _cef_render_handler_t* GetRenderHandler(_cef_client_t* _)
    {
        var ptr = (_cef_render_handler_t*)Unsafe.AsPointer(ref _renderer);
        return ptr;
    }

    [UnmanagedCallersOnly]
    static void GetViewRect(_cef_render_handler_t* renderer, _cef_browser_t* browser, _cef_rect_t* rect)
    {
        *rect = new _cef_rect_t
        {
            height = _h,
            width = _w
        };
    }

    [UnmanagedCallersOnly]
    static void OnPaint(_cef_render_handler_t* renderer, _cef_browser_t* browser,
        cef_paint_element_type_t paintElementType, nuint rectCount, _cef_rect_t* rects, void* bufferPtr, int width,
        int height)
    {
        uint* buffer = (uint*)bufferPtr;
        var array = WebRenderer.texBuffer;
        var span = MemoryMarshal.Cast<byte, uint>(WebRenderer.texBuffer);
        for (int i = 0; i < width * height; i++)
        {
            var value = buffer[i];
            span[i] = (value & 0x00_ff_00_00) >> 16 | (value & 0x_00_00_ff) << 16 | value & 0xff_00_ff_00;
        }
        
        WebRenderer.texture.SetData(array);
        WebRenderer.loadingPage = false;
        browser->@base.release(&browser->@base);
    }

    [LibraryImport("Kernel32.dll")]
    private static partial void* GetModuleHandleA(void* str);

    private const string SubprocessPath = "CefSubprocess.exe";
    private const string InitialPath = "file:///nope.html";

    [FixedAddressValueType]
    private static _cef_app_t _app;

    [FixedAddressValueType]
    private static _cef_client_t _client;

    [FixedAddressValueType]
    private static _cef_render_handler_t _renderer;

    private static _cef_browser_t* _browser;

    private static int _w = 500, _h = 500;

    public static void Initialize()
    {
        var args = new _cef_main_args_t
        {
            instance = GetModuleHandleA(null)
        };

        var stubCounter = new _cef_base_ref_counted_t
        {
            add_ref = &AddRef,
            release = &Release,
            has_one_ref = &HasOneRef,
            has_at_least_one_ref = &HasAtLeastOneRef
        };

        _app = new _cef_app_t
        {
            @base = stubCounter with { size = (nuint)sizeof(_cef_app_t) },
            get_browser_process_handler = &GetProcessHandler,
            get_resource_bundle_handler = &GetResourceBundleHandler,
            on_before_command_line_processing = &OnBeforeCommandLineProcessing,
            on_register_custom_schemes = &OnBeforeSchemaRegister,
            get_render_process_handler = &GetRenderProcessHandler
        };

        var path = Path.GetFullPath(SubprocessPath);
        var settings = new _cef_settings_t
        {
            browser_subprocess_path = new _cef_string_utf16_t
            {
                dtor = &StringDealloc,
                length = (nuint)path.Length,
                str = Utf16StringMarshaller.ConvertToUnmanaged(path)
            },
            no_sandbox = 1,
            windowless_rendering_enabled = 1,
            size = (nuint)sizeof(_cef_settings_t),
        };

        _ = Methods.cef_initialize(&args, &settings, (_cef_app_t*)Unsafe.AsPointer(ref _app), null);

        _renderer = new _cef_render_handler_t
        {
            @base = stubCounter with { size = (nuint)sizeof(_cef_render_handler_t) },
            get_view_rect = &GetViewRect,
            on_paint = &OnPaint
        };

        var windowInfo = new _cef_window_info_t
        {
            windowless_rendering_enabled = 1
        };

        _client = new _cef_client_t
        {
            @base = stubCounter with { size = (nuint)sizeof(_cef_client_t) },
            get_render_handler = &GetRenderHandler
        };
        
        var initialUrl = new _cef_string_utf16_t
        {
            dtor = &StringDealloc,
            length = (nuint)InitialPath.Length,
            str = Utf16StringMarshaller.ConvertToUnmanaged(InitialPath)
        };

        var browserSettings = new _cef_browser_settings_t
        {
            javascript_close_windows = cef_state_t.STATE_DISABLED,
            javascript_access_clipboard = cef_state_t.STATE_DISABLED,
            size = (nuint)sizeof(_cef_browser_settings_t)
        };

        _browser = CefInterop.Methods.cef_browser_host_create_browser_sync(&windowInfo, (_cef_client_t*)Unsafe.AsPointer(ref _client), &initialUrl,
            &browserSettings, null, null);
    }

    public static void SetViewport(int width, int height)
    {
        _w = width;
        _h = height;
        var host = _browser->get_host(_browser);
        host->was_resized(host);
    }

    public static void LoadURL(string urlString)
    {
        var url = new _cef_string_utf16_t
        {
            dtor = &StringDealloc,
            length = (nuint)urlString.Length,
            str = Utf16StringMarshaller.ConvertToUnmanaged(urlString)
        };
        var mainFrame = _browser->get_main_frame(_browser);
        mainFrame->load_url(mainFrame, &url);
    }

    [HarmonyILManipulator]
    [HarmonyPatch(typeof(Game1), nameof(Game1.LoadContent))]
    private static void FixInitializeIL(ILContext il)
    {
        var c = new ILCursor(il);

        c.GotoNext(x => x.MatchCall(AccessTools.DeclaredMethod(typeof(XNAWebRenderer), nameof(XNAWebRenderer.XNAWR_Initialize))));

        c.Next!.Operand = il.Import(AccessTools.DeclaredMethod(typeof(NewCefImpl), nameof(Initialize)));

        c.EmitPop();
        c.EmitPop();
        c.EmitPop();
        c.EmitPop();
    }
    
    [HarmonyILManipulator]
    [HarmonyPatch(typeof(Game1), nameof(Game1.Update))]
    private static void FixUpdateIL(ILContext il)
    {
        var c = new ILCursor(il);

        c.GotoNext(x => x.MatchCall(AccessTools.DeclaredMethod(typeof(XNAWebRenderer), nameof(XNAWebRenderer.XNAWR_Update))));

        c.Next!.Operand = il.Import(AccessTools.DeclaredMethod(typeof(Methods), nameof(Methods.cef_do_message_loop_work)));
    }
    
    [HarmonyILManipulator]
    [HarmonyPatch(typeof(WebRenderer), nameof(WebRenderer.setSize))]
    private static void FixSetViewportIL(ILContext il)
    {
        var c = new ILCursor(il);

        c.GotoNext(x => x.MatchCall(AccessTools.DeclaredMethod(typeof(XNAWebRenderer), nameof(XNAWebRenderer.XNAWR_SetViewport))));

        c.Next!.Operand = il.Import(AccessTools.DeclaredMethod(typeof(NewCefImpl), nameof(SetViewport)));
    }
    
    [HarmonyILManipulator]
    [HarmonyPatch(typeof(WebRenderer), nameof(WebRenderer.navigateTo))]
    private static void FixLoadURLIL(ILContext il)
    {
        var c = new ILCursor(il);

        c.GotoNext(x => x.MatchCall(AccessTools.DeclaredMethod(typeof(XNAWebRenderer), nameof(XNAWebRenderer.XNAWR_LoadURL))));

        c.Next!.Operand = il.Import(AccessTools.DeclaredMethod(typeof(NewCefImpl), nameof(LoadURL)));
    }
    
    [HarmonyILManipulator]
    [HarmonyPatch(typeof(Game1), nameof(Game1.UnloadContent))]
    private static void FixShutdownIL(ILContext il)
    {
        var c = new ILCursor(il);

        c.GotoNext(x => x.MatchCall(AccessTools.DeclaredMethod(typeof(XNAWebRenderer), nameof(XNAWebRenderer.XNAWR_Shutdown))));

        c.Next!.Operand = il.Import(AccessTools.DeclaredMethod(typeof(Methods), nameof(Methods.cef_shutdown)));
    }
}
