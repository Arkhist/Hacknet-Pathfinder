namespace CefInterop
{
    public unsafe partial struct _cef_load_handler_t
    {
        [NativeTypeName("cef_base_ref_counted_t")]
        public _cef_base_ref_counted_t @base;

        [NativeTypeName("void (*)(struct _cef_load_handler_t *, struct _cef_browser_t *, int, int, int) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_load_handler_t*, _cef_browser_t*, int, int, int, void> on_loading_state_change;

        [NativeTypeName("void (*)(struct _cef_load_handler_t *, struct _cef_browser_t *, struct _cef_frame_t *, cef_transition_type_t) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_load_handler_t*, _cef_browser_t*, _cef_frame_t*, cef_transition_type_t, void> on_load_start;

        [NativeTypeName("void (*)(struct _cef_load_handler_t *, struct _cef_browser_t *, struct _cef_frame_t *, int) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_load_handler_t*, _cef_browser_t*, _cef_frame_t*, int, void> on_load_end;

        [NativeTypeName("void (*)(struct _cef_load_handler_t *, struct _cef_browser_t *, struct _cef_frame_t *, cef_errorcode_t, const cef_string_t *, const cef_string_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_load_handler_t*, _cef_browser_t*, _cef_frame_t*, cef_errorcode_t, _cef_string_utf16_t*, _cef_string_utf16_t*, void> on_load_error;
    }
}
