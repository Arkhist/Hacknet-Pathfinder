namespace CefInterop
{
    public unsafe partial struct _cef_response_t
    {
        [NativeTypeName("cef_base_ref_counted_t")]
        public _cef_base_ref_counted_t @base;

        [NativeTypeName("int (*)(struct _cef_response_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_response_t*, int> is_read_only;

        [NativeTypeName("cef_errorcode_t (*)(struct _cef_response_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_response_t*, cef_errorcode_t> get_error;

        [NativeTypeName("void (*)(struct _cef_response_t *, cef_errorcode_t) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_response_t*, cef_errorcode_t, void> set_error;

        [NativeTypeName("int (*)(struct _cef_response_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_response_t*, int> get_status;

        [NativeTypeName("void (*)(struct _cef_response_t *, int) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_response_t*, int, void> set_status;

        [NativeTypeName("cef_string_userfree_t (*)(struct _cef_response_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_response_t*, _cef_string_utf16_t*> get_status_text;

        [NativeTypeName("void (*)(struct _cef_response_t *, const cef_string_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_response_t*, _cef_string_utf16_t*, void> set_status_text;

        [NativeTypeName("cef_string_userfree_t (*)(struct _cef_response_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_response_t*, _cef_string_utf16_t*> get_mime_type;

        [NativeTypeName("void (*)(struct _cef_response_t *, const cef_string_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_response_t*, _cef_string_utf16_t*, void> set_mime_type;

        [NativeTypeName("cef_string_userfree_t (*)(struct _cef_response_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_response_t*, _cef_string_utf16_t*> get_charset;

        [NativeTypeName("void (*)(struct _cef_response_t *, const cef_string_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_response_t*, _cef_string_utf16_t*, void> set_charset;

        [NativeTypeName("cef_string_userfree_t (*)(struct _cef_response_t *, const cef_string_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_response_t*, _cef_string_utf16_t*, _cef_string_utf16_t*> get_header_by_name;

        [NativeTypeName("void (*)(struct _cef_response_t *, const cef_string_t *, const cef_string_t *, int) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_response_t*, _cef_string_utf16_t*, _cef_string_utf16_t*, int, void> set_header_by_name;

        [NativeTypeName("void (*)(struct _cef_response_t *, cef_string_multimap_t) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_response_t*, _cef_string_multimap_t*, void> get_header_map;

        [NativeTypeName("void (*)(struct _cef_response_t *, cef_string_multimap_t) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_response_t*, _cef_string_multimap_t*, void> set_header_map;

        [NativeTypeName("cef_string_userfree_t (*)(struct _cef_response_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_response_t*, _cef_string_utf16_t*> get_url;

        [NativeTypeName("void (*)(struct _cef_response_t *, const cef_string_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_response_t*, _cef_string_utf16_t*, void> set_url;
    }
}
