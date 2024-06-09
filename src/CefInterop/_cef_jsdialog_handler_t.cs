namespace CefInterop
{
    public unsafe partial struct _cef_jsdialog_handler_t
    {
        [NativeTypeName("cef_base_ref_counted_t")]
        public _cef_base_ref_counted_t @base;

        [NativeTypeName("int (*)(struct _cef_jsdialog_handler_t *, struct _cef_browser_t *, const cef_string_t *, cef_jsdialog_type_t, const cef_string_t *, const cef_string_t *, struct _cef_jsdialog_callback_t *, int *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_jsdialog_handler_t*, _cef_browser_t*, _cef_string_utf16_t*, cef_jsdialog_type_t, _cef_string_utf16_t*, _cef_string_utf16_t*, _cef_jsdialog_callback_t*, int*, int> on_jsdialog;

        [NativeTypeName("int (*)(struct _cef_jsdialog_handler_t *, struct _cef_browser_t *, const cef_string_t *, int, struct _cef_jsdialog_callback_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_jsdialog_handler_t*, _cef_browser_t*, _cef_string_utf16_t*, int, _cef_jsdialog_callback_t*, int> on_before_unload_dialog;

        [NativeTypeName("void (*)(struct _cef_jsdialog_handler_t *, struct _cef_browser_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_jsdialog_handler_t*, _cef_browser_t*, void> on_reset_dialog_state;

        [NativeTypeName("void (*)(struct _cef_jsdialog_handler_t *, struct _cef_browser_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_jsdialog_handler_t*, _cef_browser_t*, void> on_dialog_closed;
    }
}
