namespace CefInterop
{
    public unsafe partial struct _cef_v8value_t
    {
        [NativeTypeName("cef_base_ref_counted_t")]
        public _cef_base_ref_counted_t @base;

        [NativeTypeName("int (*)(struct _cef_v8value_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_v8value_t*, int> is_valid;

        [NativeTypeName("int (*)(struct _cef_v8value_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_v8value_t*, int> is_undefined;

        [NativeTypeName("int (*)(struct _cef_v8value_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_v8value_t*, int> is_null;

        [NativeTypeName("int (*)(struct _cef_v8value_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_v8value_t*, int> is_bool;

        [NativeTypeName("int (*)(struct _cef_v8value_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_v8value_t*, int> is_int;

        [NativeTypeName("int (*)(struct _cef_v8value_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_v8value_t*, int> is_uint;

        [NativeTypeName("int (*)(struct _cef_v8value_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_v8value_t*, int> is_double;

        [NativeTypeName("int (*)(struct _cef_v8value_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_v8value_t*, int> is_date;

        [NativeTypeName("int (*)(struct _cef_v8value_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_v8value_t*, int> is_string;

        [NativeTypeName("int (*)(struct _cef_v8value_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_v8value_t*, int> is_object;

        [NativeTypeName("int (*)(struct _cef_v8value_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_v8value_t*, int> is_array;

        [NativeTypeName("int (*)(struct _cef_v8value_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_v8value_t*, int> is_array_buffer;

        [NativeTypeName("int (*)(struct _cef_v8value_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_v8value_t*, int> is_function;

        [NativeTypeName("int (*)(struct _cef_v8value_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_v8value_t*, int> is_promise;

        [NativeTypeName("int (*)(struct _cef_v8value_t *, struct _cef_v8value_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_v8value_t*, _cef_v8value_t*, int> is_same;

        [NativeTypeName("int (*)(struct _cef_v8value_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_v8value_t*, int> get_bool_value;

        [NativeTypeName("int32_t (*)(struct _cef_v8value_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_v8value_t*, int> get_int_value;

        [NativeTypeName("uint32_t (*)(struct _cef_v8value_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_v8value_t*, uint> get_uint_value;

        [NativeTypeName("double (*)(struct _cef_v8value_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_v8value_t*, double> get_double_value;

        [NativeTypeName("cef_basetime_t (*)(struct _cef_v8value_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_v8value_t*, _cef_basetime_t> get_date_value;

        [NativeTypeName("cef_string_userfree_t (*)(struct _cef_v8value_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_v8value_t*, _cef_string_utf16_t*> get_string_value;

        [NativeTypeName("int (*)(struct _cef_v8value_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_v8value_t*, int> is_user_created;

        [NativeTypeName("int (*)(struct _cef_v8value_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_v8value_t*, int> has_exception;

        [NativeTypeName("struct _cef_v8exception_t *(*)(struct _cef_v8value_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_v8value_t*, _cef_v8exception_t*> get_exception;

        [NativeTypeName("int (*)(struct _cef_v8value_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_v8value_t*, int> clear_exception;

        [NativeTypeName("int (*)(struct _cef_v8value_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_v8value_t*, int> will_rethrow_exceptions;

        [NativeTypeName("int (*)(struct _cef_v8value_t *, int) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_v8value_t*, int, int> set_rethrow_exceptions;

        [NativeTypeName("int (*)(struct _cef_v8value_t *, const cef_string_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_v8value_t*, _cef_string_utf16_t*, int> has_value_bykey;

        [NativeTypeName("int (*)(struct _cef_v8value_t *, int) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_v8value_t*, int, int> has_value_byindex;

        [NativeTypeName("int (*)(struct _cef_v8value_t *, const cef_string_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_v8value_t*, _cef_string_utf16_t*, int> delete_value_bykey;

        [NativeTypeName("int (*)(struct _cef_v8value_t *, int) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_v8value_t*, int, int> delete_value_byindex;

        [NativeTypeName("struct _cef_v8value_t *(*)(struct _cef_v8value_t *, const cef_string_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_v8value_t*, _cef_string_utf16_t*, _cef_v8value_t*> get_value_bykey;

        [NativeTypeName("struct _cef_v8value_t *(*)(struct _cef_v8value_t *, int) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_v8value_t*, int, _cef_v8value_t*> get_value_byindex;

        [NativeTypeName("int (*)(struct _cef_v8value_t *, const cef_string_t *, struct _cef_v8value_t *, cef_v8_propertyattribute_t) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_v8value_t*, _cef_string_utf16_t*, _cef_v8value_t*, cef_v8_propertyattribute_t, int> set_value_bykey;

        [NativeTypeName("int (*)(struct _cef_v8value_t *, int, struct _cef_v8value_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_v8value_t*, int, _cef_v8value_t*, int> set_value_byindex;

        [NativeTypeName("int (*)(struct _cef_v8value_t *, const cef_string_t *, cef_v8_accesscontrol_t, cef_v8_propertyattribute_t) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_v8value_t*, _cef_string_utf16_t*, cef_v8_accesscontrol_t, cef_v8_propertyattribute_t, int> set_value_byaccessor;

        [NativeTypeName("int (*)(struct _cef_v8value_t *, cef_string_list_t) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_v8value_t*, _cef_string_list_t*, int> get_keys;

        [NativeTypeName("int (*)(struct _cef_v8value_t *, struct _cef_base_ref_counted_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_v8value_t*, _cef_base_ref_counted_t*, int> set_user_data;

        [NativeTypeName("struct _cef_base_ref_counted_t *(*)(struct _cef_v8value_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_v8value_t*, _cef_base_ref_counted_t*> get_user_data;

        [NativeTypeName("int (*)(struct _cef_v8value_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_v8value_t*, int> get_externally_allocated_memory;

        [NativeTypeName("int (*)(struct _cef_v8value_t *, int) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_v8value_t*, int, int> adjust_externally_allocated_memory;

        [NativeTypeName("int (*)(struct _cef_v8value_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_v8value_t*, int> get_array_length;

        [NativeTypeName("struct _cef_v8array_buffer_release_callback_t *(*)(struct _cef_v8value_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_v8value_t*, _cef_v8array_buffer_release_callback_t*> get_array_buffer_release_callback;

        [NativeTypeName("int (*)(struct _cef_v8value_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_v8value_t*, int> neuter_array_buffer;

        [NativeTypeName("size_t (*)(struct _cef_v8value_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_v8value_t*, nuint> get_array_buffer_byte_length;

        [NativeTypeName("void *(*)(struct _cef_v8value_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_v8value_t*, void*> get_array_buffer_data;

        [NativeTypeName("cef_string_userfree_t (*)(struct _cef_v8value_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_v8value_t*, _cef_string_utf16_t*> get_function_name;

        [NativeTypeName("struct _cef_v8handler_t *(*)(struct _cef_v8value_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_v8value_t*, _cef_v8handler_t*> get_function_handler;

        [NativeTypeName("struct _cef_v8value_t *(*)(struct _cef_v8value_t *, struct _cef_v8value_t *, size_t, struct _cef_v8value_t *const *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_v8value_t*, _cef_v8value_t*, nuint, _cef_v8value_t**, _cef_v8value_t*> execute_function;

        [NativeTypeName("struct _cef_v8value_t *(*)(struct _cef_v8value_t *, struct _cef_v8context_t *, struct _cef_v8value_t *, size_t, struct _cef_v8value_t *const *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_v8value_t*, _cef_v8context_t*, _cef_v8value_t*, nuint, _cef_v8value_t**, _cef_v8value_t*> execute_function_with_context;

        [NativeTypeName("int (*)(struct _cef_v8value_t *, struct _cef_v8value_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_v8value_t*, _cef_v8value_t*, int> resolve_promise;

        [NativeTypeName("int (*)(struct _cef_v8value_t *, const cef_string_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_v8value_t*, _cef_string_utf16_t*, int> reject_promise;
    }
}
