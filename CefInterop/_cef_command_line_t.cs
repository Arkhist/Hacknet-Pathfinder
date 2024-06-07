namespace CefInterop
{
    public unsafe partial struct _cef_command_line_t
    {
        [NativeTypeName("cef_base_ref_counted_t")]
        public _cef_base_ref_counted_t @base;

        [NativeTypeName("int (*)(struct _cef_command_line_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_command_line_t*, int> is_valid;

        [NativeTypeName("int (*)(struct _cef_command_line_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_command_line_t*, int> is_read_only;

        [NativeTypeName("struct _cef_command_line_t *(*)(struct _cef_command_line_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_command_line_t*, _cef_command_line_t*> copy;

        [NativeTypeName("void (*)(struct _cef_command_line_t *, int, const char *const *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_command_line_t*, int, sbyte**, void> init_from_argv;

        [NativeTypeName("void (*)(struct _cef_command_line_t *, const cef_string_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_command_line_t*, _cef_string_utf16_t*, void> init_from_string;

        [NativeTypeName("void (*)(struct _cef_command_line_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_command_line_t*, void> reset;

        [NativeTypeName("void (*)(struct _cef_command_line_t *, cef_string_list_t) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_command_line_t*, _cef_string_list_t*, void> get_argv;

        [NativeTypeName("cef_string_userfree_t (*)(struct _cef_command_line_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_command_line_t*, _cef_string_utf16_t*> get_command_line_string;

        [NativeTypeName("cef_string_userfree_t (*)(struct _cef_command_line_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_command_line_t*, _cef_string_utf16_t*> get_program;

        [NativeTypeName("void (*)(struct _cef_command_line_t *, const cef_string_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_command_line_t*, _cef_string_utf16_t*, void> set_program;

        [NativeTypeName("int (*)(struct _cef_command_line_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_command_line_t*, int> has_switches;

        [NativeTypeName("int (*)(struct _cef_command_line_t *, const cef_string_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_command_line_t*, _cef_string_utf16_t*, int> has_switch;

        [NativeTypeName("cef_string_userfree_t (*)(struct _cef_command_line_t *, const cef_string_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_command_line_t*, _cef_string_utf16_t*, _cef_string_utf16_t*> get_switch_value;

        [NativeTypeName("void (*)(struct _cef_command_line_t *, cef_string_map_t) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_command_line_t*, _cef_string_map_t*, void> get_switches;

        [NativeTypeName("void (*)(struct _cef_command_line_t *, const cef_string_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_command_line_t*, _cef_string_utf16_t*, void> append_switch;

        [NativeTypeName("void (*)(struct _cef_command_line_t *, const cef_string_t *, const cef_string_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_command_line_t*, _cef_string_utf16_t*, _cef_string_utf16_t*, void> append_switch_with_value;

        [NativeTypeName("int (*)(struct _cef_command_line_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_command_line_t*, int> has_arguments;

        [NativeTypeName("void (*)(struct _cef_command_line_t *, cef_string_list_t) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_command_line_t*, _cef_string_list_t*, void> get_arguments;

        [NativeTypeName("void (*)(struct _cef_command_line_t *, const cef_string_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_command_line_t*, _cef_string_utf16_t*, void> append_argument;

        [NativeTypeName("void (*)(struct _cef_command_line_t *, const cef_string_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_command_line_t*, _cef_string_utf16_t*, void> prepend_wrapper;
    }
}
