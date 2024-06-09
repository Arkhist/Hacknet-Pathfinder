namespace CefInterop
{
    public unsafe partial struct _cef_app_t
    {
        [NativeTypeName("cef_base_ref_counted_t")]
        public _cef_base_ref_counted_t @base;

        [NativeTypeName("void (*)(struct _cef_app_t *, const cef_string_t *, struct _cef_command_line_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_app_t*, _cef_string_utf16_t*, _cef_command_line_t*, void> on_before_command_line_processing;

        [NativeTypeName("void (*)(struct _cef_app_t *, struct _cef_scheme_registrar_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_app_t*, _cef_scheme_registrar_t*, void> on_register_custom_schemes;

        [NativeTypeName("struct _cef_resource_bundle_handler_t *(*)(struct _cef_app_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_app_t*, _cef_resource_bundle_handler_t*> get_resource_bundle_handler;

        [NativeTypeName("struct _cef_browser_process_handler_t *(*)(struct _cef_app_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_app_t*, _cef_browser_process_handler_t*> get_browser_process_handler;

        [NativeTypeName("struct _cef_render_process_handler_t *(*)(struct _cef_app_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_app_t*, _cef_render_process_handler_t*> get_render_process_handler;
    }
}
