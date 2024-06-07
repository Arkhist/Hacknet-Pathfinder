namespace CefInterop
{
    public unsafe partial struct _cef_drag_data_t
    {
        [NativeTypeName("cef_base_ref_counted_t")]
        public _cef_base_ref_counted_t @base;

        [NativeTypeName("struct _cef_drag_data_t *(*)(struct _cef_drag_data_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_drag_data_t*, _cef_drag_data_t*> clone;

        [NativeTypeName("int (*)(struct _cef_drag_data_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_drag_data_t*, int> is_read_only;

        [NativeTypeName("int (*)(struct _cef_drag_data_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_drag_data_t*, int> is_link;

        [NativeTypeName("int (*)(struct _cef_drag_data_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_drag_data_t*, int> is_fragment;

        [NativeTypeName("int (*)(struct _cef_drag_data_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_drag_data_t*, int> is_file;

        [NativeTypeName("cef_string_userfree_t (*)(struct _cef_drag_data_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_drag_data_t*, _cef_string_utf16_t*> get_link_url;

        [NativeTypeName("cef_string_userfree_t (*)(struct _cef_drag_data_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_drag_data_t*, _cef_string_utf16_t*> get_link_title;

        [NativeTypeName("cef_string_userfree_t (*)(struct _cef_drag_data_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_drag_data_t*, _cef_string_utf16_t*> get_link_metadata;

        [NativeTypeName("cef_string_userfree_t (*)(struct _cef_drag_data_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_drag_data_t*, _cef_string_utf16_t*> get_fragment_text;

        [NativeTypeName("cef_string_userfree_t (*)(struct _cef_drag_data_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_drag_data_t*, _cef_string_utf16_t*> get_fragment_html;

        [NativeTypeName("cef_string_userfree_t (*)(struct _cef_drag_data_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_drag_data_t*, _cef_string_utf16_t*> get_fragment_base_url;

        [NativeTypeName("cef_string_userfree_t (*)(struct _cef_drag_data_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_drag_data_t*, _cef_string_utf16_t*> get_file_name;

        [NativeTypeName("size_t (*)(struct _cef_drag_data_t *, struct _cef_stream_writer_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_drag_data_t*, _cef_stream_writer_t*, nuint> get_file_contents;

        [NativeTypeName("int (*)(struct _cef_drag_data_t *, cef_string_list_t) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_drag_data_t*, _cef_string_list_t*, int> get_file_names;

        [NativeTypeName("int (*)(struct _cef_drag_data_t *, cef_string_list_t) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_drag_data_t*, _cef_string_list_t*, int> get_file_paths;

        [NativeTypeName("void (*)(struct _cef_drag_data_t *, const cef_string_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_drag_data_t*, _cef_string_utf16_t*, void> set_link_url;

        [NativeTypeName("void (*)(struct _cef_drag_data_t *, const cef_string_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_drag_data_t*, _cef_string_utf16_t*, void> set_link_title;

        [NativeTypeName("void (*)(struct _cef_drag_data_t *, const cef_string_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_drag_data_t*, _cef_string_utf16_t*, void> set_link_metadata;

        [NativeTypeName("void (*)(struct _cef_drag_data_t *, const cef_string_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_drag_data_t*, _cef_string_utf16_t*, void> set_fragment_text;

        [NativeTypeName("void (*)(struct _cef_drag_data_t *, const cef_string_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_drag_data_t*, _cef_string_utf16_t*, void> set_fragment_html;

        [NativeTypeName("void (*)(struct _cef_drag_data_t *, const cef_string_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_drag_data_t*, _cef_string_utf16_t*, void> set_fragment_base_url;

        [NativeTypeName("void (*)(struct _cef_drag_data_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_drag_data_t*, void> reset_file_contents;

        [NativeTypeName("void (*)(struct _cef_drag_data_t *, const cef_string_t *, const cef_string_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_drag_data_t*, _cef_string_utf16_t*, _cef_string_utf16_t*, void> add_file;

        [NativeTypeName("void (*)(struct _cef_drag_data_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_drag_data_t*, void> clear_filenames;

        [NativeTypeName("struct _cef_image_t *(*)(struct _cef_drag_data_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_drag_data_t*, _cef_image_t*> get_image;

        [NativeTypeName("cef_point_t (*)(struct _cef_drag_data_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_drag_data_t*, _cef_point_t> get_image_hotspot;

        [NativeTypeName("int (*)(struct _cef_drag_data_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_drag_data_t*, int> has_image;
    }
}
