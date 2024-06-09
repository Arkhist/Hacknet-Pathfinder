namespace CefInterop
{
    public unsafe partial struct _cef_frame_t
    {
        [NativeTypeName("cef_base_ref_counted_t")]
        public _cef_base_ref_counted_t @base;

        [NativeTypeName("int (*)(struct _cef_frame_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_frame_t*, int> is_valid;

        [NativeTypeName("void (*)(struct _cef_frame_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_frame_t*, void> undo;

        [NativeTypeName("void (*)(struct _cef_frame_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_frame_t*, void> redo;

        [NativeTypeName("void (*)(struct _cef_frame_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_frame_t*, void> cut;

        [NativeTypeName("void (*)(struct _cef_frame_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_frame_t*, void> copy;

        [NativeTypeName("void (*)(struct _cef_frame_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_frame_t*, void> paste;

        [NativeTypeName("void (*)(struct _cef_frame_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_frame_t*, void> del;

        [NativeTypeName("void (*)(struct _cef_frame_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_frame_t*, void> select_all;

        [NativeTypeName("void (*)(struct _cef_frame_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_frame_t*, void> view_source;

        [NativeTypeName("void (*)(struct _cef_frame_t *, struct _cef_string_visitor_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_frame_t*, _cef_string_visitor_t*, void> get_source;

        [NativeTypeName("void (*)(struct _cef_frame_t *, struct _cef_string_visitor_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_frame_t*, _cef_string_visitor_t*, void> get_text;

        [NativeTypeName("void (*)(struct _cef_frame_t *, struct _cef_request_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_frame_t*, _cef_request_t*, void> load_request;

        [NativeTypeName("void (*)(struct _cef_frame_t *, const cef_string_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_frame_t*, _cef_string_utf16_t*, void> load_url;

        [NativeTypeName("void (*)(struct _cef_frame_t *, const cef_string_t *, const cef_string_t *, int) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_frame_t*, _cef_string_utf16_t*, _cef_string_utf16_t*, int, void> execute_java_script;

        [NativeTypeName("int (*)(struct _cef_frame_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_frame_t*, int> is_main;

        [NativeTypeName("int (*)(struct _cef_frame_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_frame_t*, int> is_focused;

        [NativeTypeName("cef_string_userfree_t (*)(struct _cef_frame_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_frame_t*, _cef_string_utf16_t*> get_name;

        [NativeTypeName("cef_string_userfree_t (*)(struct _cef_frame_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_frame_t*, _cef_string_utf16_t*> get_identifier;

        [NativeTypeName("struct _cef_frame_t *(*)(struct _cef_frame_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_frame_t*, _cef_frame_t*> get_parent;

        [NativeTypeName("cef_string_userfree_t (*)(struct _cef_frame_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_frame_t*, _cef_string_utf16_t*> get_url;

        [NativeTypeName("struct _cef_browser_t *(*)(struct _cef_frame_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_frame_t*, _cef_browser_t*> get_browser;

        [NativeTypeName("struct _cef_v8context_t *(*)(struct _cef_frame_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_frame_t*, _cef_v8context_t*> get_v8context;

        [NativeTypeName("void (*)(struct _cef_frame_t *, struct _cef_domvisitor_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_frame_t*, _cef_domvisitor_t*, void> visit_dom;

        [NativeTypeName("struct _cef_urlrequest_t *(*)(struct _cef_frame_t *, struct _cef_request_t *, struct _cef_urlrequest_client_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_frame_t*, _cef_request_t*, _cef_urlrequest_client_t*, _cef_urlrequest_t*> create_urlrequest;

        [NativeTypeName("void (*)(struct _cef_frame_t *, cef_process_id_t, struct _cef_process_message_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_frame_t*, cef_process_id_t, _cef_process_message_t*, void> send_process_message;
    }
}
