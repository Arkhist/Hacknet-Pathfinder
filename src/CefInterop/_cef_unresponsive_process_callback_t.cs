namespace CefInterop
{
    public unsafe partial struct _cef_unresponsive_process_callback_t
    {
        [NativeTypeName("cef_base_ref_counted_t")]
        public _cef_base_ref_counted_t @base;

        [NativeTypeName("void (*)(struct _cef_unresponsive_process_callback_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_unresponsive_process_callback_t*, void> wait;

        [NativeTypeName("void (*)(struct _cef_unresponsive_process_callback_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_unresponsive_process_callback_t*, void> terminate;
    }
}
