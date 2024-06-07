namespace CefInterop
{
    public unsafe partial struct _cef_find_handler_t
    {
        [NativeTypeName("cef_base_ref_counted_t")]
        public _cef_base_ref_counted_t @base;

        [NativeTypeName("void (*)(struct _cef_find_handler_t *, struct _cef_browser_t *, int, int, const cef_rect_t *, int, int) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_find_handler_t*, _cef_browser_t*, int, int, _cef_rect_t*, int, int, void> on_find_result;
    }
}
