namespace CefInterop
{
    public unsafe partial struct _cef_binary_value_t
    {
        [NativeTypeName("cef_base_ref_counted_t")]
        public _cef_base_ref_counted_t @base;

        [NativeTypeName("int (*)(struct _cef_binary_value_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_binary_value_t*, int> is_valid;

        [NativeTypeName("int (*)(struct _cef_binary_value_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_binary_value_t*, int> is_owned;

        [NativeTypeName("int (*)(struct _cef_binary_value_t *, struct _cef_binary_value_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_binary_value_t*, _cef_binary_value_t*, int> is_same;

        [NativeTypeName("int (*)(struct _cef_binary_value_t *, struct _cef_binary_value_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_binary_value_t*, _cef_binary_value_t*, int> is_equal;

        [NativeTypeName("struct _cef_binary_value_t *(*)(struct _cef_binary_value_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_binary_value_t*, _cef_binary_value_t*> copy;

        [NativeTypeName("const void *(*)(struct _cef_binary_value_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_binary_value_t*, void*> get_raw_data;

        [NativeTypeName("size_t (*)(struct _cef_binary_value_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_binary_value_t*, nuint> get_size;

        [NativeTypeName("size_t (*)(struct _cef_binary_value_t *, void *, size_t, size_t) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_binary_value_t*, void*, nuint, nuint, nuint> get_data;
    }
}
