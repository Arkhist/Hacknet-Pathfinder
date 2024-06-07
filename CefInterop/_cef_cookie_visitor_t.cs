namespace CefInterop
{
    public unsafe partial struct _cef_cookie_visitor_t
    {
        [NativeTypeName("cef_base_ref_counted_t")]
        public _cef_base_ref_counted_t @base;

        [NativeTypeName("int (*)(struct _cef_cookie_visitor_t *, const struct _cef_cookie_t *, int, int, int *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_cookie_visitor_t*, _cef_cookie_t*, int, int, int*, int> visit;
    }
}
