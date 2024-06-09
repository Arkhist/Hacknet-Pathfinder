namespace CefInterop
{
    public unsafe partial struct _cef_navigation_entry_t
    {
        [NativeTypeName("cef_base_ref_counted_t")]
        public _cef_base_ref_counted_t @base;

        [NativeTypeName("int (*)(struct _cef_navigation_entry_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_navigation_entry_t*, int> is_valid;

        [NativeTypeName("cef_string_userfree_t (*)(struct _cef_navigation_entry_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_navigation_entry_t*, _cef_string_utf16_t*> get_url;

        [NativeTypeName("cef_string_userfree_t (*)(struct _cef_navigation_entry_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_navigation_entry_t*, _cef_string_utf16_t*> get_display_url;

        [NativeTypeName("cef_string_userfree_t (*)(struct _cef_navigation_entry_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_navigation_entry_t*, _cef_string_utf16_t*> get_original_url;

        [NativeTypeName("cef_string_userfree_t (*)(struct _cef_navigation_entry_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_navigation_entry_t*, _cef_string_utf16_t*> get_title;

        [NativeTypeName("cef_transition_type_t (*)(struct _cef_navigation_entry_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_navigation_entry_t*, cef_transition_type_t> get_transition_type;

        [NativeTypeName("int (*)(struct _cef_navigation_entry_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_navigation_entry_t*, int> has_post_data;

        [NativeTypeName("cef_basetime_t (*)(struct _cef_navigation_entry_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_navigation_entry_t*, _cef_basetime_t> get_completion_time;

        [NativeTypeName("int (*)(struct _cef_navigation_entry_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_navigation_entry_t*, int> get_http_status_code;

        [NativeTypeName("struct _cef_sslstatus_t *(*)(struct _cef_navigation_entry_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_navigation_entry_t*, _cef_sslstatus_t*> get_sslstatus;
    }
}
