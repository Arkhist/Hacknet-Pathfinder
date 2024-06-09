namespace CefInterop
{
    public unsafe partial struct _cef_life_span_handler_t
    {
        [NativeTypeName("cef_base_ref_counted_t")]
        public _cef_base_ref_counted_t @base;

        [NativeTypeName("int (*)(struct _cef_life_span_handler_t *, struct _cef_browser_t *, struct _cef_frame_t *, const cef_string_t *, const cef_string_t *, cef_window_open_disposition_t, int, const cef_popup_features_t *, struct _cef_window_info_t *, struct _cef_client_t **, struct _cef_browser_settings_t *, struct _cef_dictionary_value_t **, int *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_life_span_handler_t*, _cef_browser_t*, _cef_frame_t*, _cef_string_utf16_t*, _cef_string_utf16_t*, cef_window_open_disposition_t, int, _cef_popup_features_t*, void*, _cef_client_t**, _cef_browser_settings_t*, _cef_dictionary_value_t**, int*, int> on_before_popup;

        [NativeTypeName("void (*)(struct _cef_life_span_handler_t *, struct _cef_browser_t *, struct _cef_window_info_t *, struct _cef_client_t **, struct _cef_browser_settings_t *, struct _cef_dictionary_value_t **, int *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_life_span_handler_t*, _cef_browser_t*, void*, _cef_client_t**, _cef_browser_settings_t*, _cef_dictionary_value_t**, int*, void> on_before_dev_tools_popup;

        [NativeTypeName("void (*)(struct _cef_life_span_handler_t *, struct _cef_browser_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_life_span_handler_t*, _cef_browser_t*, void> on_after_created;

        [NativeTypeName("int (*)(struct _cef_life_span_handler_t *, struct _cef_browser_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_life_span_handler_t*, _cef_browser_t*, int> do_close;

        [NativeTypeName("void (*)(struct _cef_life_span_handler_t *, struct _cef_browser_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_life_span_handler_t*, _cef_browser_t*, void> on_before_close;
    }
}
