namespace CefInterop
{
    public unsafe partial struct _cef_string_visitor_t
    {
        [NativeTypeName("cef_base_ref_counted_t")]
        public _cef_base_ref_counted_t @base;

        [NativeTypeName("void (*)(struct _cef_string_visitor_t *, const cef_string_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_string_visitor_t*, _cef_string_utf16_t*, void> visit;
    }
}
