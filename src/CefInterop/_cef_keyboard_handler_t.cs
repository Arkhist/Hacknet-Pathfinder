namespace CefInterop
{
    public unsafe partial struct _cef_keyboard_handler_t
    {
        [NativeTypeName("cef_base_ref_counted_t")]
        public _cef_base_ref_counted_t @base;

        [NativeTypeName("int (*)(struct _cef_keyboard_handler_t *, struct _cef_browser_t *, const cef_key_event_t *, MSG *, int *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_keyboard_handler_t*, _cef_browser_t*, _cef_key_event_t*, void*, int*, int> on_pre_key_event;

        [NativeTypeName("int (*)(struct _cef_keyboard_handler_t *, struct _cef_browser_t *, const cef_key_event_t *, MSG *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_keyboard_handler_t*, _cef_browser_t*, _cef_key_event_t*, void*, int> on_key_event;
    }
}
