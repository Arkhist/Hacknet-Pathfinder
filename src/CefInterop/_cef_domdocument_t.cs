namespace CefInterop
{
    public unsafe partial struct _cef_domdocument_t
    {
        [NativeTypeName("cef_base_ref_counted_t")]
        public _cef_base_ref_counted_t @base;

        [NativeTypeName("cef_dom_document_type_t (*)(struct _cef_domdocument_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_domdocument_t*, cef_dom_document_type_t> get_type;

        [NativeTypeName("struct _cef_domnode_t *(*)(struct _cef_domdocument_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_domdocument_t*, _cef_domnode_t*> get_document;

        [NativeTypeName("struct _cef_domnode_t *(*)(struct _cef_domdocument_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_domdocument_t*, _cef_domnode_t*> get_body;

        [NativeTypeName("struct _cef_domnode_t *(*)(struct _cef_domdocument_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_domdocument_t*, _cef_domnode_t*> get_head;

        [NativeTypeName("cef_string_userfree_t (*)(struct _cef_domdocument_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_domdocument_t*, _cef_string_utf16_t*> get_title;

        [NativeTypeName("struct _cef_domnode_t *(*)(struct _cef_domdocument_t *, const cef_string_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_domdocument_t*, _cef_string_utf16_t*, _cef_domnode_t*> get_element_by_id;

        [NativeTypeName("struct _cef_domnode_t *(*)(struct _cef_domdocument_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_domdocument_t*, _cef_domnode_t*> get_focused_node;

        [NativeTypeName("int (*)(struct _cef_domdocument_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_domdocument_t*, int> has_selection;

        [NativeTypeName("int (*)(struct _cef_domdocument_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_domdocument_t*, int> get_selection_start_offset;

        [NativeTypeName("int (*)(struct _cef_domdocument_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_domdocument_t*, int> get_selection_end_offset;

        [NativeTypeName("cef_string_userfree_t (*)(struct _cef_domdocument_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_domdocument_t*, _cef_string_utf16_t*> get_selection_as_markup;

        [NativeTypeName("cef_string_userfree_t (*)(struct _cef_domdocument_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_domdocument_t*, _cef_string_utf16_t*> get_selection_as_text;

        [NativeTypeName("cef_string_userfree_t (*)(struct _cef_domdocument_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_domdocument_t*, _cef_string_utf16_t*> get_base_url;

        [NativeTypeName("cef_string_userfree_t (*)(struct _cef_domdocument_t *, const cef_string_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_domdocument_t*, _cef_string_utf16_t*, _cef_string_utf16_t*> get_complete_url;
    }
}
