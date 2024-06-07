namespace CefInterop
{
    public unsafe partial struct _cef_request_context_handler_t
    {
        [NativeTypeName("cef_base_ref_counted_t")]
        public _cef_base_ref_counted_t @base;

        [NativeTypeName("void (*)(struct _cef_request_context_handler_t *, struct _cef_request_context_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_request_context_handler_t*, _cef_request_context_t*, void> on_request_context_initialized;

        [NativeTypeName("struct _cef_resource_request_handler_t *(*)(struct _cef_request_context_handler_t *, struct _cef_browser_t *, struct _cef_frame_t *, struct _cef_request_t *, int, int, const cef_string_t *, int *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_request_context_handler_t*, _cef_browser_t*, _cef_frame_t*, _cef_request_t*, int, int, _cef_string_utf16_t*, int*, _cef_resource_request_handler_t*> get_resource_request_handler;
    }
}
