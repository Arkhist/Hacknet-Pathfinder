namespace CefInterop
{
    public unsafe partial struct _cef_request_context_t
    {
        [NativeTypeName("cef_preference_manager_t")]
        public _cef_preference_manager_t @base;

        [NativeTypeName("int (*)(struct _cef_request_context_t *, struct _cef_request_context_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_request_context_t*, _cef_request_context_t*, int> is_same;

        [NativeTypeName("int (*)(struct _cef_request_context_t *, struct _cef_request_context_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_request_context_t*, _cef_request_context_t*, int> is_sharing_with;

        [NativeTypeName("int (*)(struct _cef_request_context_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_request_context_t*, int> is_global;

        [NativeTypeName("struct _cef_request_context_handler_t *(*)(struct _cef_request_context_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_request_context_t*, _cef_request_context_handler_t*> get_handler;

        [NativeTypeName("cef_string_userfree_t (*)(struct _cef_request_context_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_request_context_t*, _cef_string_utf16_t*> get_cache_path;

        [NativeTypeName("struct _cef_cookie_manager_t *(*)(struct _cef_request_context_t *, struct _cef_completion_callback_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_request_context_t*, _cef_completion_callback_t*, _cef_cookie_manager_t*> get_cookie_manager;

        [NativeTypeName("int (*)(struct _cef_request_context_t *, const cef_string_t *, const cef_string_t *, struct _cef_scheme_handler_factory_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_request_context_t*, _cef_string_utf16_t*, _cef_string_utf16_t*, _cef_scheme_handler_factory_t*, int> register_scheme_handler_factory;

        [NativeTypeName("int (*)(struct _cef_request_context_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_request_context_t*, int> clear_scheme_handler_factories;

        [NativeTypeName("void (*)(struct _cef_request_context_t *, struct _cef_completion_callback_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_request_context_t*, _cef_completion_callback_t*, void> clear_certificate_exceptions;

        [NativeTypeName("void (*)(struct _cef_request_context_t *, struct _cef_completion_callback_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_request_context_t*, _cef_completion_callback_t*, void> clear_http_auth_credentials;

        [NativeTypeName("void (*)(struct _cef_request_context_t *, struct _cef_completion_callback_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_request_context_t*, _cef_completion_callback_t*, void> close_all_connections;

        [NativeTypeName("void (*)(struct _cef_request_context_t *, const cef_string_t *, struct _cef_resolve_callback_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_request_context_t*, _cef_string_utf16_t*, _cef_resolve_callback_t*, void> resolve_host;

        [NativeTypeName("void (*)(struct _cef_request_context_t *, const cef_string_t *, struct _cef_dictionary_value_t *, struct _cef_extension_handler_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_request_context_t*, _cef_string_utf16_t*, _cef_dictionary_value_t*, _cef_extension_handler_t*, void> load_extension;

        [NativeTypeName("int (*)(struct _cef_request_context_t *, const cef_string_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_request_context_t*, _cef_string_utf16_t*, int> did_load_extension;

        [NativeTypeName("int (*)(struct _cef_request_context_t *, const cef_string_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_request_context_t*, _cef_string_utf16_t*, int> has_extension;

        [NativeTypeName("int (*)(struct _cef_request_context_t *, cef_string_list_t) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_request_context_t*, _cef_string_list_t*, int> get_extensions;

        [NativeTypeName("struct _cef_extension_t *(*)(struct _cef_request_context_t *, const cef_string_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_request_context_t*, _cef_string_utf16_t*, _cef_extension_t*> get_extension;

        [NativeTypeName("struct _cef_media_router_t *(*)(struct _cef_request_context_t *, struct _cef_completion_callback_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_request_context_t*, _cef_completion_callback_t*, _cef_media_router_t*> get_media_router;

        [NativeTypeName("struct _cef_value_t *(*)(struct _cef_request_context_t *, const cef_string_t *, const cef_string_t *, cef_content_setting_types_t) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_request_context_t*, _cef_string_utf16_t*, _cef_string_utf16_t*, cef_content_setting_types_t, _cef_value_t*> get_website_setting;

        [NativeTypeName("void (*)(struct _cef_request_context_t *, const cef_string_t *, const cef_string_t *, cef_content_setting_types_t, struct _cef_value_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_request_context_t*, _cef_string_utf16_t*, _cef_string_utf16_t*, cef_content_setting_types_t, _cef_value_t*, void> set_website_setting;

        [NativeTypeName("cef_content_setting_values_t (*)(struct _cef_request_context_t *, const cef_string_t *, const cef_string_t *, cef_content_setting_types_t) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_request_context_t*, _cef_string_utf16_t*, _cef_string_utf16_t*, cef_content_setting_types_t, cef_content_setting_values_t> get_content_setting;

        [NativeTypeName("void (*)(struct _cef_request_context_t *, const cef_string_t *, const cef_string_t *, cef_content_setting_types_t, cef_content_setting_values_t) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_request_context_t*, _cef_string_utf16_t*, _cef_string_utf16_t*, cef_content_setting_types_t, cef_content_setting_values_t, void> set_content_setting;

        [NativeTypeName("void (*)(struct _cef_request_context_t *, cef_color_variant_t, cef_color_t) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_request_context_t*, cef_color_variant_t, uint, void> set_chrome_color_scheme;

        [NativeTypeName("cef_color_variant_t (*)(struct _cef_request_context_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_request_context_t*, cef_color_variant_t> get_chrome_color_scheme_mode;

        [NativeTypeName("cef_color_t (*)(struct _cef_request_context_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_request_context_t*, uint> get_chrome_color_scheme_color;

        [NativeTypeName("cef_color_variant_t (*)(struct _cef_request_context_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_request_context_t*, cef_color_variant_t> get_chrome_color_scheme_variant;
    }
}
