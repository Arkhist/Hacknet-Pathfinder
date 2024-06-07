namespace CefInterop
{
    public unsafe partial struct _cef_post_data_element_t
    {
        [NativeTypeName("cef_base_ref_counted_t")]
        public _cef_base_ref_counted_t @base;

        [NativeTypeName("int (*)(struct _cef_post_data_element_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_post_data_element_t*, int> is_read_only;

        [NativeTypeName("void (*)(struct _cef_post_data_element_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_post_data_element_t*, void> set_to_empty;

        [NativeTypeName("void (*)(struct _cef_post_data_element_t *, const cef_string_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_post_data_element_t*, _cef_string_utf16_t*, void> set_to_file;

        [NativeTypeName("void (*)(struct _cef_post_data_element_t *, size_t, const void *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_post_data_element_t*, nuint, void*, void> set_to_bytes;

        [NativeTypeName("cef_postdataelement_type_t (*)(struct _cef_post_data_element_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_post_data_element_t*, cef_postdataelement_type_t> get_type;

        [NativeTypeName("cef_string_userfree_t (*)(struct _cef_post_data_element_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_post_data_element_t*, _cef_string_utf16_t*> get_file;

        [NativeTypeName("size_t (*)(struct _cef_post_data_element_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_post_data_element_t*, nuint> get_bytes_count;

        [NativeTypeName("size_t (*)(struct _cef_post_data_element_t *, size_t, void *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_post_data_element_t*, nuint, void*, nuint> get_bytes;
    }
}
