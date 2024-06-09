namespace CefInterop
{
    public unsafe partial struct _cef_focus_handler_t
    {
        [NativeTypeName("cef_base_ref_counted_t")]
        public _cef_base_ref_counted_t @base;

        [NativeTypeName("void (*)(struct _cef_focus_handler_t *, struct _cef_browser_t *, int) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_focus_handler_t*, _cef_browser_t*, int, void> on_take_focus;

        [NativeTypeName("int (*)(struct _cef_focus_handler_t *, struct _cef_browser_t *, cef_focus_source_t) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_focus_handler_t*, _cef_browser_t*, cef_focus_source_t, int> on_set_focus;

        [NativeTypeName("void (*)(struct _cef_focus_handler_t *, struct _cef_browser_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_focus_handler_t*, _cef_browser_t*, void> on_got_focus;
    }
}
