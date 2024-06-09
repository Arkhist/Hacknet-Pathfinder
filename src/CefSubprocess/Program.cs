using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using CefInterop;

namespace CefSubprocess;

public static unsafe class Program
{
    [UnmanagedCallersOnly]
    static void AddRefApp(_cef_base_ref_counted_t* counter)
    {
    }

    [UnmanagedCallersOnly]
    static int ReleaseApp(_cef_base_ref_counted_t* counter)
    {
        return 1;
    }

    [UnmanagedCallersOnly]
    static int HasOneRefApp(_cef_base_ref_counted_t* _)
    {
        return 1;
    }

    [UnmanagedCallersOnly]
    static int HasAtLeastOneRefApp(_cef_base_ref_counted_t* _)
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
    
    public static int Main()
    {
        void* argsPtr;
        _cef_main_args_t_windows argsWin = default;
        _cef_main_args_t_linux argsLinux = default;
        if (OperatingSystem.IsWindows())
        {
            argsPtr = &argsWin;
            argsWin.Initialize();
        }
        else
        {
            argsPtr = &argsLinux;
            argsLinux.Initialize(Environment.ProcessPath ?? throw new Exception("abc"));
        }

        var app = new _cef_app_t
        {
            @base = new _cef_base_ref_counted_t
            {
                add_ref = &AddRefApp,
                release = &ReleaseApp,
                has_one_ref = &HasOneRefApp,
                has_at_least_one_ref = &HasAtLeastOneRefApp
            },
            get_browser_process_handler = &GetProcessHandler,
            get_resource_bundle_handler = &GetResourceBundleHandler,
            on_before_command_line_processing = &OnBeforeCommandLineProcessing,
            on_register_custom_schemes = &OnBeforeSchemaRegister,
            get_render_process_handler = &GetRenderProcessHandler
        };

        return CefInterop.Methods.cef_execute_process(argsPtr, &app, null);
    }
}
