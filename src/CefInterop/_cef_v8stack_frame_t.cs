namespace CefInterop
{
    public unsafe partial struct _cef_v8stack_frame_t
    {
        [NativeTypeName("cef_base_ref_counted_t")]
        public _cef_base_ref_counted_t @base;

        [NativeTypeName("int (*)(struct _cef_v8stack_frame_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_v8stack_frame_t*, int> is_valid;

        [NativeTypeName("cef_string_userfree_t (*)(struct _cef_v8stack_frame_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_v8stack_frame_t*, _cef_string_utf16_t*> get_script_name;

        [NativeTypeName("cef_string_userfree_t (*)(struct _cef_v8stack_frame_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_v8stack_frame_t*, _cef_string_utf16_t*> get_script_name_or_source_url;

        [NativeTypeName("cef_string_userfree_t (*)(struct _cef_v8stack_frame_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_v8stack_frame_t*, _cef_string_utf16_t*> get_function_name;

        [NativeTypeName("int (*)(struct _cef_v8stack_frame_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_v8stack_frame_t*, int> get_line_number;

        [NativeTypeName("int (*)(struct _cef_v8stack_frame_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_v8stack_frame_t*, int> get_column;

        [NativeTypeName("int (*)(struct _cef_v8stack_frame_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_v8stack_frame_t*, int> is_eval;

        [NativeTypeName("int (*)(struct _cef_v8stack_frame_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_v8stack_frame_t*, int> is_constructor;
    }
}
