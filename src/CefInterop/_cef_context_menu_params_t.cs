namespace CefInterop
{
    public unsafe partial struct _cef_context_menu_params_t
    {
        [NativeTypeName("cef_base_ref_counted_t")]
        public _cef_base_ref_counted_t @base;

        [NativeTypeName("int (*)(struct _cef_context_menu_params_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_context_menu_params_t*, int> get_xcoord;

        [NativeTypeName("int (*)(struct _cef_context_menu_params_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_context_menu_params_t*, int> get_ycoord;

        [NativeTypeName("cef_context_menu_type_flags_t (*)(struct _cef_context_menu_params_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_context_menu_params_t*, cef_context_menu_type_flags_t> get_type_flags;

        [NativeTypeName("cef_string_userfree_t (*)(struct _cef_context_menu_params_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_context_menu_params_t*, _cef_string_utf16_t*> get_link_url;

        [NativeTypeName("cef_string_userfree_t (*)(struct _cef_context_menu_params_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_context_menu_params_t*, _cef_string_utf16_t*> get_unfiltered_link_url;

        [NativeTypeName("cef_string_userfree_t (*)(struct _cef_context_menu_params_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_context_menu_params_t*, _cef_string_utf16_t*> get_source_url;

        [NativeTypeName("int (*)(struct _cef_context_menu_params_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_context_menu_params_t*, int> has_image_contents;

        [NativeTypeName("cef_string_userfree_t (*)(struct _cef_context_menu_params_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_context_menu_params_t*, _cef_string_utf16_t*> get_title_text;

        [NativeTypeName("cef_string_userfree_t (*)(struct _cef_context_menu_params_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_context_menu_params_t*, _cef_string_utf16_t*> get_page_url;

        [NativeTypeName("cef_string_userfree_t (*)(struct _cef_context_menu_params_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_context_menu_params_t*, _cef_string_utf16_t*> get_frame_url;

        [NativeTypeName("cef_string_userfree_t (*)(struct _cef_context_menu_params_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_context_menu_params_t*, _cef_string_utf16_t*> get_frame_charset;

        [NativeTypeName("cef_context_menu_media_type_t (*)(struct _cef_context_menu_params_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_context_menu_params_t*, cef_context_menu_media_type_t> get_media_type;

        [NativeTypeName("cef_context_menu_media_state_flags_t (*)(struct _cef_context_menu_params_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_context_menu_params_t*, cef_context_menu_media_state_flags_t> get_media_state_flags;

        [NativeTypeName("cef_string_userfree_t (*)(struct _cef_context_menu_params_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_context_menu_params_t*, _cef_string_utf16_t*> get_selection_text;

        [NativeTypeName("cef_string_userfree_t (*)(struct _cef_context_menu_params_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_context_menu_params_t*, _cef_string_utf16_t*> get_misspelled_word;

        [NativeTypeName("int (*)(struct _cef_context_menu_params_t *, cef_string_list_t) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_context_menu_params_t*, _cef_string_list_t*, int> get_dictionary_suggestions;

        [NativeTypeName("int (*)(struct _cef_context_menu_params_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_context_menu_params_t*, int> is_editable;

        [NativeTypeName("int (*)(struct _cef_context_menu_params_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_context_menu_params_t*, int> is_spell_check_enabled;

        [NativeTypeName("cef_context_menu_edit_state_flags_t (*)(struct _cef_context_menu_params_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_context_menu_params_t*, cef_context_menu_edit_state_flags_t> get_edit_state_flags;

        [NativeTypeName("int (*)(struct _cef_context_menu_params_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_context_menu_params_t*, int> is_custom_menu;
    }
}
