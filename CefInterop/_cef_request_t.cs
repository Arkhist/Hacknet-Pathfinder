namespace CefInterop
{
    public unsafe partial struct _cef_request_t
    {
        [NativeTypeName("cef_base_ref_counted_t")]
        public _cef_base_ref_counted_t @base;

        [NativeTypeName("int (*)(struct _cef_request_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_request_t*, int> is_read_only;

        [NativeTypeName("cef_string_userfree_t (*)(struct _cef_request_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_request_t*, _cef_string_utf16_t*> get_url;

        [NativeTypeName("void (*)(struct _cef_request_t *, const cef_string_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_request_t*, _cef_string_utf16_t*, void> set_url;

        [NativeTypeName("cef_string_userfree_t (*)(struct _cef_request_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_request_t*, _cef_string_utf16_t*> get_method;

        [NativeTypeName("void (*)(struct _cef_request_t *, const cef_string_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_request_t*, _cef_string_utf16_t*, void> set_method;

        [NativeTypeName("void (*)(struct _cef_request_t *, const cef_string_t *, cef_referrer_policy_t) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_request_t*, _cef_string_utf16_t*, cef_referrer_policy_t, void> set_referrer;

        [NativeTypeName("cef_string_userfree_t (*)(struct _cef_request_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_request_t*, _cef_string_utf16_t*> get_referrer_url;

        [NativeTypeName("cef_referrer_policy_t (*)(struct _cef_request_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_request_t*, cef_referrer_policy_t> get_referrer_policy;

        [NativeTypeName("struct _cef_post_data_t *(*)(struct _cef_request_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_request_t*, _cef_post_data_t*> get_post_data;

        [NativeTypeName("void (*)(struct _cef_request_t *, struct _cef_post_data_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_request_t*, _cef_post_data_t*, void> set_post_data;

        [NativeTypeName("void (*)(struct _cef_request_t *, cef_string_multimap_t) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_request_t*, _cef_string_multimap_t*, void> get_header_map;

        [NativeTypeName("void (*)(struct _cef_request_t *, cef_string_multimap_t) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_request_t*, _cef_string_multimap_t*, void> set_header_map;

        [NativeTypeName("cef_string_userfree_t (*)(struct _cef_request_t *, const cef_string_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_request_t*, _cef_string_utf16_t*, _cef_string_utf16_t*> get_header_by_name;

        [NativeTypeName("void (*)(struct _cef_request_t *, const cef_string_t *, const cef_string_t *, int) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_request_t*, _cef_string_utf16_t*, _cef_string_utf16_t*, int, void> set_header_by_name;

        [NativeTypeName("void (*)(struct _cef_request_t *, const cef_string_t *, const cef_string_t *, struct _cef_post_data_t *, cef_string_multimap_t) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_request_t*, _cef_string_utf16_t*, _cef_string_utf16_t*, _cef_post_data_t*, _cef_string_multimap_t*, void> set;

        [NativeTypeName("int (*)(struct _cef_request_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_request_t*, int> get_flags;

        [NativeTypeName("void (*)(struct _cef_request_t *, int) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_request_t*, int, void> set_flags;

        [NativeTypeName("cef_string_userfree_t (*)(struct _cef_request_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_request_t*, _cef_string_utf16_t*> get_first_party_for_cookies;

        [NativeTypeName("void (*)(struct _cef_request_t *, const cef_string_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_request_t*, _cef_string_utf16_t*, void> set_first_party_for_cookies;

        [NativeTypeName("cef_resource_type_t (*)(struct _cef_request_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_request_t*, cef_resource_type_t> get_resource_type;

        [NativeTypeName("cef_transition_type_t (*)(struct _cef_request_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_request_t*, cef_transition_type_t> get_transition_type;

        [NativeTypeName("uint64_t (*)(struct _cef_request_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_request_t*, ulong> get_identifier;
    }
}
