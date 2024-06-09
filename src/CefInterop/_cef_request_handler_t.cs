namespace CefInterop
{
    public unsafe partial struct _cef_request_handler_t
    {
        [NativeTypeName("cef_base_ref_counted_t")]
        public _cef_base_ref_counted_t @base;

        [NativeTypeName("int (*)(struct _cef_request_handler_t *, struct _cef_browser_t *, struct _cef_frame_t *, struct _cef_request_t *, int, int) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_request_handler_t*, _cef_browser_t*, _cef_frame_t*, _cef_request_t*, int, int, int> on_before_browse;

        [NativeTypeName("int (*)(struct _cef_request_handler_t *, struct _cef_browser_t *, struct _cef_frame_t *, const cef_string_t *, cef_window_open_disposition_t, int) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_request_handler_t*, _cef_browser_t*, _cef_frame_t*, _cef_string_utf16_t*, cef_window_open_disposition_t, int, int> on_open_urlfrom_tab;

        [NativeTypeName("struct _cef_resource_request_handler_t *(*)(struct _cef_request_handler_t *, struct _cef_browser_t *, struct _cef_frame_t *, struct _cef_request_t *, int, int, const cef_string_t *, int *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_request_handler_t*, _cef_browser_t*, _cef_frame_t*, _cef_request_t*, int, int, _cef_string_utf16_t*, int*, _cef_resource_request_handler_t*> get_resource_request_handler;

        [NativeTypeName("int (*)(struct _cef_request_handler_t *, struct _cef_browser_t *, const cef_string_t *, int, const cef_string_t *, int, const cef_string_t *, const cef_string_t *, struct _cef_auth_callback_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_request_handler_t*, _cef_browser_t*, _cef_string_utf16_t*, int, _cef_string_utf16_t*, int, _cef_string_utf16_t*, _cef_string_utf16_t*, _cef_auth_callback_t*, int> get_auth_credentials;

        [NativeTypeName("int (*)(struct _cef_request_handler_t *, struct _cef_browser_t *, cef_errorcode_t, const cef_string_t *, struct _cef_sslinfo_t *, struct _cef_callback_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_request_handler_t*, _cef_browser_t*, cef_errorcode_t, _cef_string_utf16_t*, _cef_sslinfo_t*, _cef_callback_t*, int> on_certificate_error;

        [NativeTypeName("int (*)(struct _cef_request_handler_t *, struct _cef_browser_t *, int, const cef_string_t *, int, size_t, struct _cef_x509certificate_t *const *, struct _cef_select_client_certificate_callback_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_request_handler_t*, _cef_browser_t*, int, _cef_string_utf16_t*, int, nuint, _cef_x509certificate_t**, _cef_select_client_certificate_callback_t*, int> on_select_client_certificate;

        [NativeTypeName("void (*)(struct _cef_request_handler_t *, struct _cef_browser_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_request_handler_t*, _cef_browser_t*, void> on_render_view_ready;

        [NativeTypeName("int (*)(struct _cef_request_handler_t *, struct _cef_browser_t *, struct _cef_unresponsive_process_callback_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_request_handler_t*, _cef_browser_t*, _cef_unresponsive_process_callback_t*, int> on_render_process_unresponsive;

        [NativeTypeName("void (*)(struct _cef_request_handler_t *, struct _cef_browser_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_request_handler_t*, _cef_browser_t*, void> on_render_process_responsive;

        [NativeTypeName("void (*)(struct _cef_request_handler_t *, struct _cef_browser_t *, cef_termination_status_t, int, const cef_string_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_request_handler_t*, _cef_browser_t*, cef_termination_status_t, int, _cef_string_utf16_t*, void> on_render_process_terminated;

        [NativeTypeName("void (*)(struct _cef_request_handler_t *, struct _cef_browser_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_request_handler_t*, _cef_browser_t*, void> on_document_available_in_main_frame;
    }
}
