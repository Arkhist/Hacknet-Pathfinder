namespace CefInterop
{
    public unsafe partial struct _cef_menu_model_t
    {
        [NativeTypeName("cef_base_ref_counted_t")]
        public _cef_base_ref_counted_t @base;

        [NativeTypeName("int (*)(struct _cef_menu_model_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_menu_model_t*, int> is_sub_menu;

        [NativeTypeName("int (*)(struct _cef_menu_model_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_menu_model_t*, int> clear;

        [NativeTypeName("size_t (*)(struct _cef_menu_model_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_menu_model_t*, nuint> get_count;

        [NativeTypeName("int (*)(struct _cef_menu_model_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_menu_model_t*, int> add_separator;

        [NativeTypeName("int (*)(struct _cef_menu_model_t *, int, const cef_string_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_menu_model_t*, int, _cef_string_utf16_t*, int> add_item;

        [NativeTypeName("int (*)(struct _cef_menu_model_t *, int, const cef_string_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_menu_model_t*, int, _cef_string_utf16_t*, int> add_check_item;

        [NativeTypeName("int (*)(struct _cef_menu_model_t *, int, const cef_string_t *, int) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_menu_model_t*, int, _cef_string_utf16_t*, int, int> add_radio_item;

        [NativeTypeName("struct _cef_menu_model_t *(*)(struct _cef_menu_model_t *, int, const cef_string_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_menu_model_t*, int, _cef_string_utf16_t*, _cef_menu_model_t*> add_sub_menu;

        [NativeTypeName("int (*)(struct _cef_menu_model_t *, size_t) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_menu_model_t*, nuint, int> insert_separator_at;

        [NativeTypeName("int (*)(struct _cef_menu_model_t *, size_t, int, const cef_string_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_menu_model_t*, nuint, int, _cef_string_utf16_t*, int> insert_item_at;

        [NativeTypeName("int (*)(struct _cef_menu_model_t *, size_t, int, const cef_string_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_menu_model_t*, nuint, int, _cef_string_utf16_t*, int> insert_check_item_at;

        [NativeTypeName("int (*)(struct _cef_menu_model_t *, size_t, int, const cef_string_t *, int) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_menu_model_t*, nuint, int, _cef_string_utf16_t*, int, int> insert_radio_item_at;

        [NativeTypeName("struct _cef_menu_model_t *(*)(struct _cef_menu_model_t *, size_t, int, const cef_string_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_menu_model_t*, nuint, int, _cef_string_utf16_t*, _cef_menu_model_t*> insert_sub_menu_at;

        [NativeTypeName("int (*)(struct _cef_menu_model_t *, int) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_menu_model_t*, int, int> remove;

        [NativeTypeName("int (*)(struct _cef_menu_model_t *, size_t) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_menu_model_t*, nuint, int> remove_at;

        [NativeTypeName("int (*)(struct _cef_menu_model_t *, int) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_menu_model_t*, int, int> get_index_of;

        [NativeTypeName("int (*)(struct _cef_menu_model_t *, size_t) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_menu_model_t*, nuint, int> get_command_id_at;

        [NativeTypeName("int (*)(struct _cef_menu_model_t *, size_t, int) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_menu_model_t*, nuint, int, int> set_command_id_at;

        [NativeTypeName("cef_string_userfree_t (*)(struct _cef_menu_model_t *, int) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_menu_model_t*, int, _cef_string_utf16_t*> get_label;

        [NativeTypeName("cef_string_userfree_t (*)(struct _cef_menu_model_t *, size_t) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_menu_model_t*, nuint, _cef_string_utf16_t*> get_label_at;

        [NativeTypeName("int (*)(struct _cef_menu_model_t *, int, const cef_string_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_menu_model_t*, int, _cef_string_utf16_t*, int> set_label;

        [NativeTypeName("int (*)(struct _cef_menu_model_t *, size_t, const cef_string_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_menu_model_t*, nuint, _cef_string_utf16_t*, int> set_label_at;

        [NativeTypeName("cef_menu_item_type_t (*)(struct _cef_menu_model_t *, int) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_menu_model_t*, int, cef_menu_item_type_t> get_type;

        [NativeTypeName("cef_menu_item_type_t (*)(struct _cef_menu_model_t *, size_t) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_menu_model_t*, nuint, cef_menu_item_type_t> get_type_at;

        [NativeTypeName("int (*)(struct _cef_menu_model_t *, int) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_menu_model_t*, int, int> get_group_id;

        [NativeTypeName("int (*)(struct _cef_menu_model_t *, size_t) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_menu_model_t*, nuint, int> get_group_id_at;

        [NativeTypeName("int (*)(struct _cef_menu_model_t *, int, int) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_menu_model_t*, int, int, int> set_group_id;

        [NativeTypeName("int (*)(struct _cef_menu_model_t *, size_t, int) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_menu_model_t*, nuint, int, int> set_group_id_at;

        [NativeTypeName("struct _cef_menu_model_t *(*)(struct _cef_menu_model_t *, int) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_menu_model_t*, int, _cef_menu_model_t*> get_sub_menu;

        [NativeTypeName("struct _cef_menu_model_t *(*)(struct _cef_menu_model_t *, size_t) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_menu_model_t*, nuint, _cef_menu_model_t*> get_sub_menu_at;

        [NativeTypeName("int (*)(struct _cef_menu_model_t *, int) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_menu_model_t*, int, int> is_visible;

        [NativeTypeName("int (*)(struct _cef_menu_model_t *, size_t) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_menu_model_t*, nuint, int> is_visible_at;

        [NativeTypeName("int (*)(struct _cef_menu_model_t *, int, int) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_menu_model_t*, int, int, int> set_visible;

        [NativeTypeName("int (*)(struct _cef_menu_model_t *, size_t, int) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_menu_model_t*, nuint, int, int> set_visible_at;

        [NativeTypeName("int (*)(struct _cef_menu_model_t *, int) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_menu_model_t*, int, int> is_enabled;

        [NativeTypeName("int (*)(struct _cef_menu_model_t *, size_t) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_menu_model_t*, nuint, int> is_enabled_at;

        [NativeTypeName("int (*)(struct _cef_menu_model_t *, int, int) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_menu_model_t*, int, int, int> set_enabled;

        [NativeTypeName("int (*)(struct _cef_menu_model_t *, size_t, int) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_menu_model_t*, nuint, int, int> set_enabled_at;

        [NativeTypeName("int (*)(struct _cef_menu_model_t *, int) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_menu_model_t*, int, int> is_checked;

        [NativeTypeName("int (*)(struct _cef_menu_model_t *, size_t) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_menu_model_t*, nuint, int> is_checked_at;

        [NativeTypeName("int (*)(struct _cef_menu_model_t *, int, int) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_menu_model_t*, int, int, int> set_checked;

        [NativeTypeName("int (*)(struct _cef_menu_model_t *, size_t, int) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_menu_model_t*, nuint, int, int> set_checked_at;

        [NativeTypeName("int (*)(struct _cef_menu_model_t *, int) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_menu_model_t*, int, int> has_accelerator;

        [NativeTypeName("int (*)(struct _cef_menu_model_t *, size_t) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_menu_model_t*, nuint, int> has_accelerator_at;

        [NativeTypeName("int (*)(struct _cef_menu_model_t *, int, int, int, int, int) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_menu_model_t*, int, int, int, int, int, int> set_accelerator;

        [NativeTypeName("int (*)(struct _cef_menu_model_t *, size_t, int, int, int, int) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_menu_model_t*, nuint, int, int, int, int, int> set_accelerator_at;

        [NativeTypeName("int (*)(struct _cef_menu_model_t *, int) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_menu_model_t*, int, int> remove_accelerator;

        [NativeTypeName("int (*)(struct _cef_menu_model_t *, size_t) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_menu_model_t*, nuint, int> remove_accelerator_at;

        [NativeTypeName("int (*)(struct _cef_menu_model_t *, int, int *, int *, int *, int *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_menu_model_t*, int, int*, int*, int*, int*, int> get_accelerator;

        [NativeTypeName("int (*)(struct _cef_menu_model_t *, size_t, int *, int *, int *, int *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_menu_model_t*, nuint, int*, int*, int*, int*, int> get_accelerator_at;

        [NativeTypeName("int (*)(struct _cef_menu_model_t *, int, cef_menu_color_type_t, cef_color_t) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_menu_model_t*, int, cef_menu_color_type_t, uint, int> set_color;

        [NativeTypeName("int (*)(struct _cef_menu_model_t *, int, cef_menu_color_type_t, cef_color_t) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_menu_model_t*, int, cef_menu_color_type_t, uint, int> set_color_at;

        [NativeTypeName("int (*)(struct _cef_menu_model_t *, int, cef_menu_color_type_t, cef_color_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_menu_model_t*, int, cef_menu_color_type_t, uint*, int> get_color;

        [NativeTypeName("int (*)(struct _cef_menu_model_t *, int, cef_menu_color_type_t, cef_color_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_menu_model_t*, int, cef_menu_color_type_t, uint*, int> get_color_at;

        [NativeTypeName("int (*)(struct _cef_menu_model_t *, int, const cef_string_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_menu_model_t*, int, _cef_string_utf16_t*, int> set_font_list;

        [NativeTypeName("int (*)(struct _cef_menu_model_t *, int, const cef_string_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_menu_model_t*, int, _cef_string_utf16_t*, int> set_font_list_at;
    }
}
