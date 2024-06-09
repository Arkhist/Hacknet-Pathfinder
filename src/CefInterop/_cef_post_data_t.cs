namespace CefInterop
{
    public unsafe partial struct _cef_post_data_t
    {
        [NativeTypeName("cef_base_ref_counted_t")]
        public _cef_base_ref_counted_t @base;

        [NativeTypeName("int (*)(struct _cef_post_data_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_post_data_t*, int> is_read_only;

        [NativeTypeName("int (*)(struct _cef_post_data_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_post_data_t*, int> has_excluded_elements;

        [NativeTypeName("size_t (*)(struct _cef_post_data_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_post_data_t*, nuint> get_element_count;

        [NativeTypeName("void (*)(struct _cef_post_data_t *, size_t *, struct _cef_post_data_element_t **) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_post_data_t*, nuint*, _cef_post_data_element_t**, void> get_elements;

        [NativeTypeName("int (*)(struct _cef_post_data_t *, struct _cef_post_data_element_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_post_data_t*, _cef_post_data_element_t*, int> remove_element;

        [NativeTypeName("int (*)(struct _cef_post_data_t *, struct _cef_post_data_element_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_post_data_t*, _cef_post_data_element_t*, int> add_element;

        [NativeTypeName("void (*)(struct _cef_post_data_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_post_data_t*, void> remove_elements;
    }
}
