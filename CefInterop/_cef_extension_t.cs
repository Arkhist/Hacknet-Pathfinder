namespace CefInterop
{
    public unsafe partial struct _cef_extension_t
    {
        [NativeTypeName("cef_base_ref_counted_t")]
        public _cef_base_ref_counted_t @base;

        [NativeTypeName("cef_string_userfree_t (*)(struct _cef_extension_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_extension_t*, _cef_string_utf16_t*> get_identifier;

        [NativeTypeName("cef_string_userfree_t (*)(struct _cef_extension_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_extension_t*, _cef_string_utf16_t*> get_path;

        [NativeTypeName("struct _cef_dictionary_value_t *(*)(struct _cef_extension_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_extension_t*, _cef_dictionary_value_t*> get_manifest;

        [NativeTypeName("int (*)(struct _cef_extension_t *, struct _cef_extension_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_extension_t*, _cef_extension_t*, int> is_same;

        [NativeTypeName("struct _cef_extension_handler_t *(*)(struct _cef_extension_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_extension_t*, _cef_extension_handler_t*> get_handler;

        [NativeTypeName("struct _cef_request_context_t *(*)(struct _cef_extension_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_extension_t*, _cef_request_context_t*> get_loader_context;

        [NativeTypeName("int (*)(struct _cef_extension_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_extension_t*, int> is_loaded;

        [NativeTypeName("void (*)(struct _cef_extension_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_extension_t*, void> unload;
    }
}
