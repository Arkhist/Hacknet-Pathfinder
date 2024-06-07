namespace CefInterop
{
    public unsafe partial struct _cef_jsdialog_callback_t
    {
        [NativeTypeName("cef_base_ref_counted_t")]
        public _cef_base_ref_counted_t @base;

        [NativeTypeName("void (*)(struct _cef_jsdialog_callback_t *, int, const cef_string_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_jsdialog_callback_t*, int, _cef_string_utf16_t*, void> cont;
    }
}
