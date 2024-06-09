namespace CefInterop
{
    public unsafe partial struct _cef_domnode_t
    {
        [NativeTypeName("cef_base_ref_counted_t")]
        public _cef_base_ref_counted_t @base;

        [NativeTypeName("cef_dom_node_type_t (*)(struct _cef_domnode_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_domnode_t*, cef_dom_node_type_t> get_type;

        [NativeTypeName("int (*)(struct _cef_domnode_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_domnode_t*, int> is_text;

        [NativeTypeName("int (*)(struct _cef_domnode_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_domnode_t*, int> is_element;

        [NativeTypeName("int (*)(struct _cef_domnode_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_domnode_t*, int> is_editable;

        [NativeTypeName("int (*)(struct _cef_domnode_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_domnode_t*, int> is_form_control_element;

        [NativeTypeName("cef_dom_form_control_type_t (*)(struct _cef_domnode_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_domnode_t*, cef_dom_form_control_type_t> get_form_control_element_type;

        [NativeTypeName("int (*)(struct _cef_domnode_t *, struct _cef_domnode_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_domnode_t*, _cef_domnode_t*, int> is_same;

        [NativeTypeName("cef_string_userfree_t (*)(struct _cef_domnode_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_domnode_t*, _cef_string_utf16_t*> get_name;

        [NativeTypeName("cef_string_userfree_t (*)(struct _cef_domnode_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_domnode_t*, _cef_string_utf16_t*> get_value;

        [NativeTypeName("int (*)(struct _cef_domnode_t *, const cef_string_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_domnode_t*, _cef_string_utf16_t*, int> set_value;

        [NativeTypeName("cef_string_userfree_t (*)(struct _cef_domnode_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_domnode_t*, _cef_string_utf16_t*> get_as_markup;

        [NativeTypeName("struct _cef_domdocument_t *(*)(struct _cef_domnode_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_domnode_t*, _cef_domdocument_t*> get_document;

        [NativeTypeName("struct _cef_domnode_t *(*)(struct _cef_domnode_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_domnode_t*, _cef_domnode_t*> get_parent;

        [NativeTypeName("struct _cef_domnode_t *(*)(struct _cef_domnode_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_domnode_t*, _cef_domnode_t*> get_previous_sibling;

        [NativeTypeName("struct _cef_domnode_t *(*)(struct _cef_domnode_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_domnode_t*, _cef_domnode_t*> get_next_sibling;

        [NativeTypeName("int (*)(struct _cef_domnode_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_domnode_t*, int> has_children;

        [NativeTypeName("struct _cef_domnode_t *(*)(struct _cef_domnode_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_domnode_t*, _cef_domnode_t*> get_first_child;

        [NativeTypeName("struct _cef_domnode_t *(*)(struct _cef_domnode_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_domnode_t*, _cef_domnode_t*> get_last_child;

        [NativeTypeName("cef_string_userfree_t (*)(struct _cef_domnode_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_domnode_t*, _cef_string_utf16_t*> get_element_tag_name;

        [NativeTypeName("int (*)(struct _cef_domnode_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_domnode_t*, int> has_element_attributes;

        [NativeTypeName("int (*)(struct _cef_domnode_t *, const cef_string_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_domnode_t*, _cef_string_utf16_t*, int> has_element_attribute;

        [NativeTypeName("cef_string_userfree_t (*)(struct _cef_domnode_t *, const cef_string_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_domnode_t*, _cef_string_utf16_t*, _cef_string_utf16_t*> get_element_attribute;

        [NativeTypeName("void (*)(struct _cef_domnode_t *, cef_string_map_t) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_domnode_t*, _cef_string_map_t*, void> get_element_attributes;

        [NativeTypeName("int (*)(struct _cef_domnode_t *, const cef_string_t *, const cef_string_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_domnode_t*, _cef_string_utf16_t*, _cef_string_utf16_t*, int> set_element_attribute;

        [NativeTypeName("cef_string_userfree_t (*)(struct _cef_domnode_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_domnode_t*, _cef_string_utf16_t*> get_element_inner_text;

        [NativeTypeName("cef_rect_t (*)(struct _cef_domnode_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_domnode_t*, _cef_rect_t> get_element_bounds;
    }
}
