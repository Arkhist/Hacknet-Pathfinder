namespace CefInterop
{
    public unsafe partial struct _cef_value_t
    {
        [NativeTypeName("cef_base_ref_counted_t")]
        public _cef_base_ref_counted_t @base;

        [NativeTypeName("int (*)(struct _cef_value_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_value_t*, int> is_valid;

        [NativeTypeName("int (*)(struct _cef_value_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_value_t*, int> is_owned;

        [NativeTypeName("int (*)(struct _cef_value_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_value_t*, int> is_read_only;

        [NativeTypeName("int (*)(struct _cef_value_t *, struct _cef_value_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_value_t*, _cef_value_t*, int> is_same;

        [NativeTypeName("int (*)(struct _cef_value_t *, struct _cef_value_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_value_t*, _cef_value_t*, int> is_equal;

        [NativeTypeName("struct _cef_value_t *(*)(struct _cef_value_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_value_t*, _cef_value_t*> copy;

        [NativeTypeName("cef_value_type_t (*)(struct _cef_value_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_value_t*, cef_value_type_t> get_type;

        [NativeTypeName("int (*)(struct _cef_value_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_value_t*, int> get_bool;

        [NativeTypeName("int (*)(struct _cef_value_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_value_t*, int> get_int;

        [NativeTypeName("double (*)(struct _cef_value_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_value_t*, double> get_double;

        [NativeTypeName("cef_string_userfree_t (*)(struct _cef_value_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_value_t*, _cef_string_utf16_t*> get_string;

        [NativeTypeName("struct _cef_binary_value_t *(*)(struct _cef_value_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_value_t*, _cef_binary_value_t*> get_binary;

        [NativeTypeName("struct _cef_dictionary_value_t *(*)(struct _cef_value_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_value_t*, _cef_dictionary_value_t*> get_dictionary;

        [NativeTypeName("struct _cef_list_value_t *(*)(struct _cef_value_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_value_t*, _cef_list_value_t*> get_list;

        [NativeTypeName("int (*)(struct _cef_value_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_value_t*, int> set_null;

        [NativeTypeName("int (*)(struct _cef_value_t *, int) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_value_t*, int, int> set_bool;

        [NativeTypeName("int (*)(struct _cef_value_t *, int) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_value_t*, int, int> set_int;

        [NativeTypeName("int (*)(struct _cef_value_t *, double) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_value_t*, double, int> set_double;

        [NativeTypeName("int (*)(struct _cef_value_t *, const cef_string_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_value_t*, _cef_string_utf16_t*, int> set_string;

        [NativeTypeName("int (*)(struct _cef_value_t *, struct _cef_binary_value_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_value_t*, _cef_binary_value_t*, int> set_binary;

        [NativeTypeName("int (*)(struct _cef_value_t *, struct _cef_dictionary_value_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_value_t*, _cef_dictionary_value_t*, int> set_dictionary;

        [NativeTypeName("int (*)(struct _cef_value_t *, struct _cef_list_value_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_value_t*, _cef_list_value_t*, int> set_list;
    }
}
