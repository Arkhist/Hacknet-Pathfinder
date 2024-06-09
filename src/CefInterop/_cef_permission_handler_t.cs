namespace CefInterop
{
    public unsafe partial struct _cef_permission_handler_t
    {
        [NativeTypeName("cef_base_ref_counted_t")]
        public _cef_base_ref_counted_t @base;

        [NativeTypeName("int (*)(struct _cef_permission_handler_t *, struct _cef_browser_t *, struct _cef_frame_t *, const cef_string_t *, uint32_t, struct _cef_media_access_callback_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_permission_handler_t*, _cef_browser_t*, _cef_frame_t*, _cef_string_utf16_t*, uint, _cef_media_access_callback_t*, int> on_request_media_access_permission;

        [NativeTypeName("int (*)(struct _cef_permission_handler_t *, struct _cef_browser_t *, uint64_t, const cef_string_t *, uint32_t, struct _cef_permission_prompt_callback_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_permission_handler_t*, _cef_browser_t*, ulong, _cef_string_utf16_t*, uint, _cef_permission_prompt_callback_t*, int> on_show_permission_prompt;

        [NativeTypeName("void (*)(struct _cef_permission_handler_t *, struct _cef_browser_t *, uint64_t, cef_permission_request_result_t) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_permission_handler_t*, _cef_browser_t*, ulong, cef_permission_request_result_t, void> on_dismiss_permission_prompt;
    }
}
