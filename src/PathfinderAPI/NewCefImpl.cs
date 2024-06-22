using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using BepInEx.Logging;
using CefInterop;
using Hacknet;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using MonoMod.Cil;
using Pathfinder.Options;
using Steamworks;

namespace Pathfinder;

[HarmonyPatch]
internal static unsafe partial class NewCefImpl
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
    static void OnBeforeCommandLineProcessing(_cef_app_t* app, _cef_string_utf16_t* something, _cef_command_line_t* commandLine)
    {
        if (OperatingSystem.IsLinux())
        {
            const string thing = "disable-gpu-sandbox";
            var newSwitch = new _cef_string_utf16_t
            {
                dtor = &StringDealloc,
                length = (nuint)thing.Length,
                str = Utf16StringMarshaller.ConvertToUnmanaged(thing)
            };
            commandLine->append_switch(commandLine, &newSwitch);
        }
    }

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

    private const string SubprocessPathWindows = "CefSubprocess.exe";
    private const string SubprocessPathLinux = "CefSubprocess";
    private const string InitialPath = "file:///nope.html";

    [FixedAddressValueType]
    private static _cef_app_t _app;

    [FixedAddressValueType]
    private static _cef_client_t _client;

    [FixedAddressValueType]
    private static _cef_render_handler_t _renderer;

    private static _cef_browser_t* _browser;

    private static int _w = 500, _h = 500;

    private static HHTMLBrowser _steamBrowser = HHTMLBrowser.Invalid;
    private static IDisposable[] _callbackHolder;

    private enum CefKind
    {
        Invalid = 0,
        Cef,
        Steam
    }

    private static CefKind _cefKind;

    public static void Initialize()
    {
        if (!PathfinderOptions.ForceCef.Value && PathfinderAPIPlugin.GameIsSteamVersion && PlatformAPISettings.Running)
        {
            InitializeSteam();
            _cefKind = CefKind.Steam;
        }
        else
        {
            InitializeCef();
            _cefKind = CefKind.Cef;
        }
        
        Logger.Log(LogLevel.Debug, $"Initiated Cef using kind: {_cefKind}");
    }

    private static void InitializeSteam()
    {
        SteamHTMLSurface.Init();
        _callbackHolder = [
            Callback<HTML_StartRequest_t>.Create(req => SteamHTMLSurface.AllowStartRequest(req.unBrowserHandle, true)),
            Callback<HTML_JSAlert_t>.Create(alert => SteamHTMLSurface.JSDialogResponse(alert.unBrowserHandle, true)),
            Callback<HTML_JSConfirm_t>.Create(confirm => SteamHTMLSurface.JSDialogResponse(confirm.unBrowserHandle, true)),
            Callback<HTML_FileOpenDialog_t>.Create(file => SteamHTMLSurface.FileLoadDialogResponse(file.unBrowserHandle, 0)),
            Callback<HTML_NeedsPaint_t>.Create(paint =>
            {
                var buffer = (uint*)paint.pBGRA;
                var span = MemoryMarshal.Cast<byte, uint>(WebRenderer.texBuffer);
                for (int i = 0; i < span.Length; i++)
                {
                    var value = buffer[i];
                    span[i] = (value & 0x00_ff_00_00) >> 16 | (value & 0x_00_00_ff) << 16 | value & 0xff_00_ff_00;
                }
                WebRenderer.texture.SetData(WebRenderer.texBuffer);
                WebRenderer.loadingPage = false;
            })
        ];
        using var browserResult = CallResult<HTML_BrowserReady_t>.Create((browser, _) => _steamBrowser = browser.unBrowserHandle);
        browserResult.Set(SteamHTMLSurface.CreateBrowser(null, null));
        while (_steamBrowser == HHTMLBrowser.Invalid)
        {
            SteamAPI.RunCallbacks();
        }
    }
    
    public static void InitializeCef()
    {
        void* argsPtr;
        _cef_main_args_t_windows argsWin;
        _cef_main_args_t_linux argsLinux = default;
        if (OperatingSystem.IsWindows())
        {
            argsPtr = &argsWin;
            argsWin.Initialize();
        }
        else
        {
            argsPtr = &argsLinux;
            argsLinux.Initialize(typeof(Program).Assembly.Location);
        }
        
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
            on_before_command_line_processing = &OnBeforeCommandLineProcessing,
        };

        var path = OperatingSystem.IsWindows() ? SubprocessPathWindows : SubprocessPathLinux;
        path = Path.GetFullPath(path);
        var cachePath = Path.Combine(Path.GetFullPath(Directory.GetCurrentDirectory()), "cef-cache");
        var settings = new _cef_settings_t
        {
            browser_subprocess_path = new _cef_string_utf16_t
            {
                dtor = &StringDealloc,
                length = (nuint)path.Length,
                str = Utf16StringMarshaller.ConvertToUnmanaged(path)
            },
            root_cache_path = new _cef_string_utf16_t {
                dtor = &StringDealloc,
                length = (nuint)cachePath.Length,
                str = Utf16StringMarshaller.ConvertToUnmanaged(cachePath)
            },
            no_sandbox = 1,
            windowless_rendering_enabled = 1,
            chrome_runtime = 1,
            size = (nuint)sizeof(_cef_settings_t),
        };

        _ = Methods.cef_initialize(argsPtr, &settings, (_cef_app_t*)Unsafe.AsPointer(ref _app), null);

        _renderer = new _cef_render_handler_t
        {
            @base = stubCounter with { size = (nuint)sizeof(_cef_render_handler_t) },
            get_view_rect = &GetViewRect,
            on_paint = &OnPaint
        };

        void* windowInfoPtr;
        _cef_window_info_t_windows winWindow;
        _cef_window_info_t_linux linWindow;
        if (OperatingSystem.IsWindows())
        {
            winWindow = new _cef_window_info_t_windows
            {
                windowless_rendering_enabled = 1,
                runtime_style = cef_runtime_style_t.CEF_RUNTIME_STYLE_ALLOY
            };
            windowInfoPtr = &winWindow;
        }
        else
        {
            linWindow = new _cef_window_info_t_linux
            {
                windowless_rendering_enabled = 1,
                runtime_style = cef_runtime_style_t.CEF_RUNTIME_STYLE_ALLOY
            };
            windowInfoPtr = &linWindow;
        }

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

        _browser = CefInterop.Methods.cef_browser_host_create_browser_sync(windowInfoPtr, (_cef_client_t*)Unsafe.AsPointer(ref _client), &initialUrl,
            &browserSettings, null, null);

        if (OperatingSystem.IsLinux())
        {
            argsLinux.Dispose();
        }
    }

    private static void Update()
    {
        switch (_cefKind)
        {
            case CefKind.Cef:
                Methods.cef_do_message_loop_work();
                break;
            case CefKind.Steam:
                SteamAPI.RunCallbacks();
                break;
        }
    }

    private static void SetViewport(int width, int height)
    {
        switch (_cefKind)
        {
            case CefKind.Cef:
                _w = width;
                _h = height;
                var host = _browser->get_host(_browser);
                host->was_resized(host);
                break;
            case CefKind.Steam:
                SteamHTMLSurface.SetSize(_steamBrowser, (uint)width, (uint)height);
                break;
        }
    }

    private static void LoadURL(string urlString)
    {
        switch (_cefKind)
        {
            case CefKind.Cef:
                var url = new _cef_string_utf16_t
                {
                    dtor = &StringDealloc,
                    length = (nuint)urlString.Length,
                    str = Utf16StringMarshaller.ConvertToUnmanaged(urlString)
                };
                var mainFrame = _browser->get_main_frame(_browser);
                mainFrame->load_url(mainFrame, &url);
                break;
            case CefKind.Steam:
                SteamHTMLSurface.LoadURL(_steamBrowser, urlString, null);
                break;
        }
    }

    private static void Shutdown()
    {
        switch (_cefKind)
        {
            case CefKind.Cef:
                _browser->@base.release(&_browser->@base);
                Methods.cef_shutdown();
                break;
            case CefKind.Steam:
                SteamHTMLSurface.RemoveBrowser(_steamBrowser);
                foreach (var callback in _callbackHolder)
                {
                    callback.Dispose();
                }
                _callbackHolder = [];
                SteamHTMLSurface.Shutdown();
                break;
        }
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

        c.Next!.Operand = il.Import(AccessTools.DeclaredMethod(typeof(NewCefImpl), nameof(Update)));
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

        c.Next!.Operand = il.Import(AccessTools.DeclaredMethod(typeof(NewCefImpl), nameof(Shutdown)));
    }
}
