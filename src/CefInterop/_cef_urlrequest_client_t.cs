namespace CefInterop
{
    public unsafe partial struct _cef_urlrequest_client_t
    {
        [NativeTypeName("cef_base_ref_counted_t")]
        public _cef_base_ref_counted_t @base;

        [NativeTypeName("void (*)(struct _cef_urlrequest_client_t *, struct _cef_urlrequest_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_urlrequest_client_t*, _cef_urlrequest_t*, void> on_request_complete;

        [NativeTypeName("void (*)(struct _cef_urlrequest_client_t *, struct _cef_urlrequest_t *, int64_t, int64_t) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_urlrequest_client_t*, _cef_urlrequest_t*, long, long, void> on_upload_progress;

        [NativeTypeName("void (*)(struct _cef_urlrequest_client_t *, struct _cef_urlrequest_t *, int64_t, int64_t) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_urlrequest_client_t*, _cef_urlrequest_t*, long, long, void> on_download_progress;

        [NativeTypeName("void (*)(struct _cef_urlrequest_client_t *, struct _cef_urlrequest_t *, const void *, size_t) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_urlrequest_client_t*, _cef_urlrequest_t*, void*, nuint, void> on_download_data;

        [NativeTypeName("int (*)(struct _cef_urlrequest_client_t *, int, const cef_string_t *, int, const cef_string_t *, const cef_string_t *, struct _cef_auth_callback_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_urlrequest_client_t*, int, _cef_string_utf16_t*, int, _cef_string_utf16_t*, _cef_string_utf16_t*, _cef_auth_callback_t*, int> get_auth_credentials;
    }
}
