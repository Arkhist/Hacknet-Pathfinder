namespace CefInterop
{
    public unsafe partial struct _cef_display_handler_t
    {
        [NativeTypeName("cef_base_ref_counted_t")]
        public _cef_base_ref_counted_t @base;

        [NativeTypeName("void (*)(struct _cef_display_handler_t *, struct _cef_browser_t *, struct _cef_frame_t *, const cef_string_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_display_handler_t*, _cef_browser_t*, _cef_frame_t*, _cef_string_utf16_t*, void> on_address_change;

        [NativeTypeName("void (*)(struct _cef_display_handler_t *, struct _cef_browser_t *, const cef_string_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_display_handler_t*, _cef_browser_t*, _cef_string_utf16_t*, void> on_title_change;

        [NativeTypeName("void (*)(struct _cef_display_handler_t *, struct _cef_browser_t *, cef_string_list_t) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_display_handler_t*, _cef_browser_t*, _cef_string_list_t*, void> on_favicon_urlchange;

        [NativeTypeName("void (*)(struct _cef_display_handler_t *, struct _cef_browser_t *, int) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_display_handler_t*, _cef_browser_t*, int, void> on_fullscreen_mode_change;

        [NativeTypeName("int (*)(struct _cef_display_handler_t *, struct _cef_browser_t *, cef_string_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_display_handler_t*, _cef_browser_t*, _cef_string_utf16_t*, int> on_tooltip;

        [NativeTypeName("void (*)(struct _cef_display_handler_t *, struct _cef_browser_t *, const cef_string_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_display_handler_t*, _cef_browser_t*, _cef_string_utf16_t*, void> on_status_message;

        [NativeTypeName("int (*)(struct _cef_display_handler_t *, struct _cef_browser_t *, cef_log_severity_t, const cef_string_t *, const cef_string_t *, int) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_display_handler_t*, _cef_browser_t*, cef_log_severity_t, _cef_string_utf16_t*, _cef_string_utf16_t*, int, int> on_console_message;

        [NativeTypeName("int (*)(struct _cef_display_handler_t *, struct _cef_browser_t *, const cef_size_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_display_handler_t*, _cef_browser_t*, _cef_size_t*, int> on_auto_resize;

        [NativeTypeName("void (*)(struct _cef_display_handler_t *, struct _cef_browser_t *, double) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_display_handler_t*, _cef_browser_t*, double, void> on_loading_progress_change;

        [NativeTypeName("int (*)(struct _cef_display_handler_t *, struct _cef_browser_t *, HCURSOR, cef_cursor_type_t, const cef_cursor_info_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_display_handler_t*, _cef_browser_t*, void*, cef_cursor_type_t, _cef_cursor_info_t*, int> on_cursor_change;

        [NativeTypeName("void (*)(struct _cef_display_handler_t *, struct _cef_browser_t *, int, int) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_display_handler_t*, _cef_browser_t*, int, int, void> on_media_access_change;
    }
}
