namespace CefInterop
{
    public unsafe partial struct _cef_urlrequest_t
    {
        [NativeTypeName("cef_base_ref_counted_t")]
        public _cef_base_ref_counted_t @base;

        [NativeTypeName("struct _cef_request_t *(*)(struct _cef_urlrequest_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_urlrequest_t*, _cef_request_t*> get_request;

        [NativeTypeName("struct _cef_urlrequest_client_t *(*)(struct _cef_urlrequest_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_urlrequest_t*, _cef_urlrequest_client_t*> get_client;

        [NativeTypeName("cef_urlrequest_status_t (*)(struct _cef_urlrequest_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_urlrequest_t*, cef_urlrequest_status_t> get_request_status;

        [NativeTypeName("cef_errorcode_t (*)(struct _cef_urlrequest_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_urlrequest_t*, cef_errorcode_t> get_request_error;

        [NativeTypeName("struct _cef_response_t *(*)(struct _cef_urlrequest_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_urlrequest_t*, _cef_response_t*> get_response;

        [NativeTypeName("int (*)(struct _cef_urlrequest_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_urlrequest_t*, int> response_was_cached;

        [NativeTypeName("void (*)(struct _cef_urlrequest_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_urlrequest_t*, void> cancel;
    }
}
