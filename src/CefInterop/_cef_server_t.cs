namespace CefInterop
{
    public unsafe partial struct _cef_server_t
    {
        [NativeTypeName("cef_base_ref_counted_t")]
        public _cef_base_ref_counted_t @base;

        [NativeTypeName("struct _cef_task_runner_t *(*)(struct _cef_server_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_server_t*, _cef_task_runner_t*> get_task_runner;

        [NativeTypeName("void (*)(struct _cef_server_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_server_t*, void> shutdown;

        [NativeTypeName("int (*)(struct _cef_server_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_server_t*, int> is_running;

        [NativeTypeName("cef_string_userfree_t (*)(struct _cef_server_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_server_t*, _cef_string_utf16_t*> get_address;

        [NativeTypeName("int (*)(struct _cef_server_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_server_t*, int> has_connection;

        [NativeTypeName("int (*)(struct _cef_server_t *, int) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_server_t*, int, int> is_valid_connection;

        [NativeTypeName("void (*)(struct _cef_server_t *, int, const cef_string_t *, const void *, size_t) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_server_t*, int, _cef_string_utf16_t*, void*, nuint, void> send_http200response;

        [NativeTypeName("void (*)(struct _cef_server_t *, int) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_server_t*, int, void> send_http404response;

        [NativeTypeName("void (*)(struct _cef_server_t *, int, const cef_string_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_server_t*, int, _cef_string_utf16_t*, void> send_http500response;

        [NativeTypeName("void (*)(struct _cef_server_t *, int, int, const cef_string_t *, int64_t, cef_string_multimap_t) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_server_t*, int, int, _cef_string_utf16_t*, long, _cef_string_multimap_t*, void> send_http_response;

        [NativeTypeName("void (*)(struct _cef_server_t *, int, const void *, size_t) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_server_t*, int, void*, nuint, void> send_raw_data;

        [NativeTypeName("void (*)(struct _cef_server_t *, int) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_server_t*, int, void> close_connection;

        [NativeTypeName("void (*)(struct _cef_server_t *, int, const void *, size_t) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_server_t*, int, void*, nuint, void> send_web_socket_message;
    }
}
