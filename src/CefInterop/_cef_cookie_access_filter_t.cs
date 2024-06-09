namespace CefInterop
{
    public unsafe partial struct _cef_cookie_access_filter_t
    {
        [NativeTypeName("cef_base_ref_counted_t")]
        public _cef_base_ref_counted_t @base;

        [NativeTypeName("int (*)(struct _cef_cookie_access_filter_t *, struct _cef_browser_t *, struct _cef_frame_t *, struct _cef_request_t *, const struct _cef_cookie_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_cookie_access_filter_t*, _cef_browser_t*, _cef_frame_t*, _cef_request_t*, _cef_cookie_t*, int> can_send_cookie;

        [NativeTypeName("int (*)(struct _cef_cookie_access_filter_t *, struct _cef_browser_t *, struct _cef_frame_t *, struct _cef_request_t *, struct _cef_response_t *, const struct _cef_cookie_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_cookie_access_filter_t*, _cef_browser_t*, _cef_frame_t*, _cef_request_t*, _cef_response_t*, _cef_cookie_t*, int> can_save_cookie;
    }
}
