namespace CefInterop
{
    public unsafe partial struct _cef_print_dialog_callback_t
    {
        [NativeTypeName("cef_base_ref_counted_t")]
        public _cef_base_ref_counted_t @base;

        [NativeTypeName("void (*)(struct _cef_print_dialog_callback_t *, struct _cef_print_settings_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_print_dialog_callback_t*, _cef_print_settings_t*, void> cont;

        [NativeTypeName("void (*)(struct _cef_print_dialog_callback_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_print_dialog_callback_t*, void> cancel;
    }
}
