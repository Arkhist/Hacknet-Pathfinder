namespace CefInterop
{
    public unsafe partial struct _cef_extension_handler_t
    {
        [NativeTypeName("cef_base_ref_counted_t")]
        public _cef_base_ref_counted_t @base;

        [NativeTypeName("void (*)(struct _cef_extension_handler_t *, cef_errorcode_t) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_extension_handler_t*, cef_errorcode_t, void> on_extension_load_failed;

        [NativeTypeName("void (*)(struct _cef_extension_handler_t *, struct _cef_extension_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_extension_handler_t*, _cef_extension_t*, void> on_extension_loaded;

        [NativeTypeName("void (*)(struct _cef_extension_handler_t *, struct _cef_extension_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_extension_handler_t*, _cef_extension_t*, void> on_extension_unloaded;

        [NativeTypeName("int (*)(struct _cef_extension_handler_t *, struct _cef_extension_t *, const cef_string_t *, struct _cef_client_t **, struct _cef_browser_settings_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_extension_handler_t*, _cef_extension_t*, _cef_string_utf16_t*, _cef_client_t**, _cef_browser_settings_t*, int> on_before_background_browser;

        [NativeTypeName("int (*)(struct _cef_extension_handler_t *, struct _cef_extension_t *, struct _cef_browser_t *, struct _cef_browser_t *, int, const cef_string_t *, int, struct _cef_window_info_t *, struct _cef_client_t **, struct _cef_browser_settings_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_extension_handler_t*, _cef_extension_t*, _cef_browser_t*, _cef_browser_t*, int, _cef_string_utf16_t*, int, _cef_window_info_t*, _cef_client_t**, _cef_browser_settings_t*, int> on_before_browser;

        [NativeTypeName("struct _cef_browser_t *(*)(struct _cef_extension_handler_t *, struct _cef_extension_t *, struct _cef_browser_t *, int) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_extension_handler_t*, _cef_extension_t*, _cef_browser_t*, int, _cef_browser_t*> get_active_browser;

        [NativeTypeName("int (*)(struct _cef_extension_handler_t *, struct _cef_extension_t *, struct _cef_browser_t *, int, struct _cef_browser_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_extension_handler_t*, _cef_extension_t*, _cef_browser_t*, int, _cef_browser_t*, int> can_access_browser;

        [NativeTypeName("int (*)(struct _cef_extension_handler_t *, struct _cef_extension_t *, struct _cef_browser_t *, const cef_string_t *, struct _cef_get_extension_resource_callback_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_extension_handler_t*, _cef_extension_t*, _cef_browser_t*, _cef_string_utf16_t*, _cef_get_extension_resource_callback_t*, int> get_extension_resource;
    }
}
