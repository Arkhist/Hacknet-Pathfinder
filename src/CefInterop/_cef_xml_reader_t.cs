namespace CefInterop
{
    public unsafe partial struct _cef_xml_reader_t
    {
        [NativeTypeName("cef_base_ref_counted_t")]
        public _cef_base_ref_counted_t @base;

        [NativeTypeName("int (*)(struct _cef_xml_reader_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_xml_reader_t*, int> move_to_next_node;

        [NativeTypeName("int (*)(struct _cef_xml_reader_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_xml_reader_t*, int> close;

        [NativeTypeName("int (*)(struct _cef_xml_reader_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_xml_reader_t*, int> has_error;

        [NativeTypeName("cef_string_userfree_t (*)(struct _cef_xml_reader_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_xml_reader_t*, _cef_string_utf16_t*> get_error;

        [NativeTypeName("cef_xml_node_type_t (*)(struct _cef_xml_reader_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_xml_reader_t*, cef_xml_node_type_t> get_type;

        [NativeTypeName("int (*)(struct _cef_xml_reader_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_xml_reader_t*, int> get_depth;

        [NativeTypeName("cef_string_userfree_t (*)(struct _cef_xml_reader_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_xml_reader_t*, _cef_string_utf16_t*> get_local_name;

        [NativeTypeName("cef_string_userfree_t (*)(struct _cef_xml_reader_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_xml_reader_t*, _cef_string_utf16_t*> get_prefix;

        [NativeTypeName("cef_string_userfree_t (*)(struct _cef_xml_reader_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_xml_reader_t*, _cef_string_utf16_t*> get_qualified_name;

        [NativeTypeName("cef_string_userfree_t (*)(struct _cef_xml_reader_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_xml_reader_t*, _cef_string_utf16_t*> get_namespace_uri;

        [NativeTypeName("cef_string_userfree_t (*)(struct _cef_xml_reader_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_xml_reader_t*, _cef_string_utf16_t*> get_base_uri;

        [NativeTypeName("cef_string_userfree_t (*)(struct _cef_xml_reader_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_xml_reader_t*, _cef_string_utf16_t*> get_xml_lang;

        [NativeTypeName("int (*)(struct _cef_xml_reader_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_xml_reader_t*, int> is_empty_element;

        [NativeTypeName("int (*)(struct _cef_xml_reader_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_xml_reader_t*, int> has_value;

        [NativeTypeName("cef_string_userfree_t (*)(struct _cef_xml_reader_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_xml_reader_t*, _cef_string_utf16_t*> get_value;

        [NativeTypeName("int (*)(struct _cef_xml_reader_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_xml_reader_t*, int> has_attributes;

        [NativeTypeName("size_t (*)(struct _cef_xml_reader_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_xml_reader_t*, nuint> get_attribute_count;

        [NativeTypeName("cef_string_userfree_t (*)(struct _cef_xml_reader_t *, int) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_xml_reader_t*, int, _cef_string_utf16_t*> get_attribute_byindex;

        [NativeTypeName("cef_string_userfree_t (*)(struct _cef_xml_reader_t *, const cef_string_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_xml_reader_t*, _cef_string_utf16_t*, _cef_string_utf16_t*> get_attribute_byqname;

        [NativeTypeName("cef_string_userfree_t (*)(struct _cef_xml_reader_t *, const cef_string_t *, const cef_string_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_xml_reader_t*, _cef_string_utf16_t*, _cef_string_utf16_t*, _cef_string_utf16_t*> get_attribute_bylname;

        [NativeTypeName("cef_string_userfree_t (*)(struct _cef_xml_reader_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_xml_reader_t*, _cef_string_utf16_t*> get_inner_xml;

        [NativeTypeName("cef_string_userfree_t (*)(struct _cef_xml_reader_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_xml_reader_t*, _cef_string_utf16_t*> get_outer_xml;

        [NativeTypeName("int (*)(struct _cef_xml_reader_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_xml_reader_t*, int> get_line_number;

        [NativeTypeName("int (*)(struct _cef_xml_reader_t *, int) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_xml_reader_t*, int, int> move_to_attribute_byindex;

        [NativeTypeName("int (*)(struct _cef_xml_reader_t *, const cef_string_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_xml_reader_t*, _cef_string_utf16_t*, int> move_to_attribute_byqname;

        [NativeTypeName("int (*)(struct _cef_xml_reader_t *, const cef_string_t *, const cef_string_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_xml_reader_t*, _cef_string_utf16_t*, _cef_string_utf16_t*, int> move_to_attribute_bylname;

        [NativeTypeName("int (*)(struct _cef_xml_reader_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_xml_reader_t*, int> move_to_first_attribute;

        [NativeTypeName("int (*)(struct _cef_xml_reader_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_xml_reader_t*, int> move_to_next_attribute;

        [NativeTypeName("int (*)(struct _cef_xml_reader_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_xml_reader_t*, int> move_to_carrying_element;
    }
}
