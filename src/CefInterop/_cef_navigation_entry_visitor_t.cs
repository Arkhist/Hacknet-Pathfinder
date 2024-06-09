namespace CefInterop
{
    public unsafe partial struct _cef_navigation_entry_visitor_t
    {
        [NativeTypeName("cef_base_ref_counted_t")]
        public _cef_base_ref_counted_t @base;

        [NativeTypeName("int (*)(struct _cef_navigation_entry_visitor_t *, struct _cef_navigation_entry_t *, int, int, int) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_navigation_entry_visitor_t*, _cef_navigation_entry_t*, int, int, int, int> visit;
    }
}
