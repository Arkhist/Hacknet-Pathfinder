namespace CefInterop
{
    public unsafe partial struct _cef_dialog_handler_t
    {
        [NativeTypeName("cef_base_ref_counted_t")]
        public _cef_base_ref_counted_t @base;

        [NativeTypeName("int (*)(struct _cef_dialog_handler_t *, struct _cef_browser_t *, cef_file_dialog_mode_t, const cef_string_t *, const cef_string_t *, cef_string_list_t, struct _cef_file_dialog_callback_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_dialog_handler_t*, _cef_browser_t*, cef_file_dialog_mode_t, _cef_string_utf16_t*, _cef_string_utf16_t*, _cef_string_list_t*, _cef_file_dialog_callback_t*, int> on_file_dialog;
    }
}
