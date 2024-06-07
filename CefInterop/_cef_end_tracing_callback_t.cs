namespace CefInterop
{
    public unsafe partial struct _cef_end_tracing_callback_t
    {
        [NativeTypeName("cef_base_ref_counted_t")]
        public _cef_base_ref_counted_t @base;

        [NativeTypeName("void (*)(struct _cef_end_tracing_callback_t *, const cef_string_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_end_tracing_callback_t*, _cef_string_utf16_t*, void> on_end_tracing_complete;
    }
}
