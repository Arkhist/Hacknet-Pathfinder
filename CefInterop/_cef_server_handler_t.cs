namespace CefInterop
{
    public unsafe partial struct _cef_server_handler_t
    {
        [NativeTypeName("cef_base_ref_counted_t")]
        public _cef_base_ref_counted_t @base;

        [NativeTypeName("void (*)(struct _cef_server_handler_t *, struct _cef_server_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_server_handler_t*, _cef_server_t*, void> on_server_created;

        [NativeTypeName("void (*)(struct _cef_server_handler_t *, struct _cef_server_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_server_handler_t*, _cef_server_t*, void> on_server_destroyed;

        [NativeTypeName("void (*)(struct _cef_server_handler_t *, struct _cef_server_t *, int) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_server_handler_t*, _cef_server_t*, int, void> on_client_connected;

        [NativeTypeName("void (*)(struct _cef_server_handler_t *, struct _cef_server_t *, int) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_server_handler_t*, _cef_server_t*, int, void> on_client_disconnected;

        [NativeTypeName("void (*)(struct _cef_server_handler_t *, struct _cef_server_t *, int, const cef_string_t *, struct _cef_request_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_server_handler_t*, _cef_server_t*, int, _cef_string_utf16_t*, _cef_request_t*, void> on_http_request;

        [NativeTypeName("void (*)(struct _cef_server_handler_t *, struct _cef_server_t *, int, const cef_string_t *, struct _cef_request_t *, struct _cef_callback_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_server_handler_t*, _cef_server_t*, int, _cef_string_utf16_t*, _cef_request_t*, _cef_callback_t*, void> on_web_socket_request;

        [NativeTypeName("void (*)(struct _cef_server_handler_t *, struct _cef_server_t *, int) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_server_handler_t*, _cef_server_t*, int, void> on_web_socket_connected;

        [NativeTypeName("void (*)(struct _cef_server_handler_t *, struct _cef_server_t *, int, const void *, size_t) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_server_handler_t*, _cef_server_t*, int, void*, nuint, void> on_web_socket_message;
    }
}
