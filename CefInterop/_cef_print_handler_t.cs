namespace CefInterop
{
    public unsafe partial struct _cef_print_handler_t
    {
        [NativeTypeName("cef_base_ref_counted_t")]
        public _cef_base_ref_counted_t @base;

        [NativeTypeName("void (*)(struct _cef_print_handler_t *, struct _cef_browser_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_print_handler_t*, _cef_browser_t*, void> on_print_start;

        [NativeTypeName("void (*)(struct _cef_print_handler_t *, struct _cef_browser_t *, struct _cef_print_settings_t *, int) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_print_handler_t*, _cef_browser_t*, _cef_print_settings_t*, int, void> on_print_settings;

        [NativeTypeName("int (*)(struct _cef_print_handler_t *, struct _cef_browser_t *, int, struct _cef_print_dialog_callback_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_print_handler_t*, _cef_browser_t*, int, _cef_print_dialog_callback_t*, int> on_print_dialog;

        [NativeTypeName("int (*)(struct _cef_print_handler_t *, struct _cef_browser_t *, const cef_string_t *, const cef_string_t *, struct _cef_print_job_callback_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_print_handler_t*, _cef_browser_t*, _cef_string_utf16_t*, _cef_string_utf16_t*, _cef_print_job_callback_t*, int> on_print_job;

        [NativeTypeName("void (*)(struct _cef_print_handler_t *, struct _cef_browser_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_print_handler_t*, _cef_browser_t*, void> on_print_reset;

        [NativeTypeName("cef_size_t (*)(struct _cef_print_handler_t *, struct _cef_browser_t *, int) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_print_handler_t*, _cef_browser_t*, int, _cef_size_t> get_pdf_paper_size;
    }
}
