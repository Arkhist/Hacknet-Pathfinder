namespace CefInterop
{
    public unsafe partial struct _cef_browser_t
    {
        [NativeTypeName("cef_base_ref_counted_t")]
        public _cef_base_ref_counted_t @base;

        [NativeTypeName("int (*)(struct _cef_browser_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_t*, int> is_valid;

        [NativeTypeName("struct _cef_browser_host_t *(*)(struct _cef_browser_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_t*, _cef_browser_host_t*> get_host;

        [NativeTypeName("int (*)(struct _cef_browser_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_t*, int> can_go_back;

        [NativeTypeName("void (*)(struct _cef_browser_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_t*, void> go_back;

        [NativeTypeName("int (*)(struct _cef_browser_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_t*, int> can_go_forward;

        [NativeTypeName("void (*)(struct _cef_browser_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_t*, void> go_forward;

        [NativeTypeName("int (*)(struct _cef_browser_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_t*, int> is_loading;

        [NativeTypeName("void (*)(struct _cef_browser_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_t*, void> reload;

        [NativeTypeName("void (*)(struct _cef_browser_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_t*, void> reload_ignore_cache;

        [NativeTypeName("void (*)(struct _cef_browser_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_t*, void> stop_load;

        [NativeTypeName("int (*)(struct _cef_browser_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_t*, int> get_identifier;

        [NativeTypeName("int (*)(struct _cef_browser_t *, struct _cef_browser_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_t*, _cef_browser_t*, int> is_same;

        [NativeTypeName("int (*)(struct _cef_browser_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_t*, int> is_popup;

        [NativeTypeName("int (*)(struct _cef_browser_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_t*, int> has_document;

        [NativeTypeName("struct _cef_frame_t *(*)(struct _cef_browser_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_t*, _cef_frame_t*> get_main_frame;

        [NativeTypeName("struct _cef_frame_t *(*)(struct _cef_browser_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_t*, _cef_frame_t*> get_focused_frame;

        [NativeTypeName("struct _cef_frame_t *(*)(struct _cef_browser_t *, const cef_string_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_t*, _cef_string_utf16_t*, _cef_frame_t*> get_frame_by_identifier;

        [NativeTypeName("struct _cef_frame_t *(*)(struct _cef_browser_t *, const cef_string_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_t*, _cef_string_utf16_t*, _cef_frame_t*> get_frame_by_name;

        [NativeTypeName("size_t (*)(struct _cef_browser_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_t*, nuint> get_frame_count;

        [NativeTypeName("void (*)(struct _cef_browser_t *, cef_string_list_t) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_t*, _cef_string_list_t*, void> get_frame_identifiers;

        [NativeTypeName("void (*)(struct _cef_browser_t *, cef_string_list_t) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_t*, _cef_string_list_t*, void> get_frame_names;
    }
}
