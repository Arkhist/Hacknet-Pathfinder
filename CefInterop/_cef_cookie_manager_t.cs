namespace CefInterop
{
    public unsafe partial struct _cef_cookie_manager_t
    {
        [NativeTypeName("cef_base_ref_counted_t")]
        public _cef_base_ref_counted_t @base;

        [NativeTypeName("int (*)(struct _cef_cookie_manager_t *, struct _cef_cookie_visitor_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_cookie_manager_t*, _cef_cookie_visitor_t*, int> visit_all_cookies;

        [NativeTypeName("int (*)(struct _cef_cookie_manager_t *, const cef_string_t *, int, struct _cef_cookie_visitor_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_cookie_manager_t*, _cef_string_utf16_t*, int, _cef_cookie_visitor_t*, int> visit_url_cookies;

        [NativeTypeName("int (*)(struct _cef_cookie_manager_t *, const cef_string_t *, const struct _cef_cookie_t *, struct _cef_set_cookie_callback_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_cookie_manager_t*, _cef_string_utf16_t*, _cef_cookie_t*, _cef_set_cookie_callback_t*, int> set_cookie;

        [NativeTypeName("int (*)(struct _cef_cookie_manager_t *, const cef_string_t *, const cef_string_t *, struct _cef_delete_cookies_callback_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_cookie_manager_t*, _cef_string_utf16_t*, _cef_string_utf16_t*, _cef_delete_cookies_callback_t*, int> delete_cookies;

        [NativeTypeName("int (*)(struct _cef_cookie_manager_t *, struct _cef_completion_callback_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_cookie_manager_t*, _cef_completion_callback_t*, int> flush_store;
    }
}
