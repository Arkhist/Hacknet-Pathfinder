namespace CefInterop
{
    public unsafe partial struct _cef_run_context_menu_callback_t
    {
        [NativeTypeName("cef_base_ref_counted_t")]
        public _cef_base_ref_counted_t @base;

        [NativeTypeName("void (*)(struct _cef_run_context_menu_callback_t *, int, cef_event_flags_t) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_run_context_menu_callback_t*, int, cef_event_flags_t, void> cont;

        [NativeTypeName("void (*)(struct _cef_run_context_menu_callback_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_run_context_menu_callback_t*, void> cancel;
    }
}
