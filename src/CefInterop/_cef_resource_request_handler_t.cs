namespace CefInterop
{
    public unsafe partial struct _cef_resource_request_handler_t
    {
        [NativeTypeName("cef_base_ref_counted_t")]
        public _cef_base_ref_counted_t @base;

        [NativeTypeName("struct _cef_cookie_access_filter_t *(*)(struct _cef_resource_request_handler_t *, struct _cef_browser_t *, struct _cef_frame_t *, struct _cef_request_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_resource_request_handler_t*, _cef_browser_t*, _cef_frame_t*, _cef_request_t*, _cef_cookie_access_filter_t*> get_cookie_access_filter;

        [NativeTypeName("cef_return_value_t (*)(struct _cef_resource_request_handler_t *, struct _cef_browser_t *, struct _cef_frame_t *, struct _cef_request_t *, struct _cef_callback_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_resource_request_handler_t*, _cef_browser_t*, _cef_frame_t*, _cef_request_t*, _cef_callback_t*, cef_return_value_t> on_before_resource_load;

        [NativeTypeName("struct _cef_resource_handler_t *(*)(struct _cef_resource_request_handler_t *, struct _cef_browser_t *, struct _cef_frame_t *, struct _cef_request_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_resource_request_handler_t*, _cef_browser_t*, _cef_frame_t*, _cef_request_t*, _cef_resource_handler_t*> get_resource_handler;

        [NativeTypeName("void (*)(struct _cef_resource_request_handler_t *, struct _cef_browser_t *, struct _cef_frame_t *, struct _cef_request_t *, struct _cef_response_t *, cef_string_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_resource_request_handler_t*, _cef_browser_t*, _cef_frame_t*, _cef_request_t*, _cef_response_t*, _cef_string_utf16_t*, void> on_resource_redirect;

        [NativeTypeName("int (*)(struct _cef_resource_request_handler_t *, struct _cef_browser_t *, struct _cef_frame_t *, struct _cef_request_t *, struct _cef_response_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_resource_request_handler_t*, _cef_browser_t*, _cef_frame_t*, _cef_request_t*, _cef_response_t*, int> on_resource_response;

        [NativeTypeName("struct _cef_response_filter_t *(*)(struct _cef_resource_request_handler_t *, struct _cef_browser_t *, struct _cef_frame_t *, struct _cef_request_t *, struct _cef_response_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_resource_request_handler_t*, _cef_browser_t*, _cef_frame_t*, _cef_request_t*, _cef_response_t*, _cef_response_filter_t*> get_resource_response_filter;

        [NativeTypeName("void (*)(struct _cef_resource_request_handler_t *, struct _cef_browser_t *, struct _cef_frame_t *, struct _cef_request_t *, struct _cef_response_t *, cef_urlrequest_status_t, int64_t) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_resource_request_handler_t*, _cef_browser_t*, _cef_frame_t*, _cef_request_t*, _cef_response_t*, cef_urlrequest_status_t, long, void> on_resource_load_complete;

        [NativeTypeName("void (*)(struct _cef_resource_request_handler_t *, struct _cef_browser_t *, struct _cef_frame_t *, struct _cef_request_t *, int *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_resource_request_handler_t*, _cef_browser_t*, _cef_frame_t*, _cef_request_t*, int*, void> on_protocol_execution;
    }
}
