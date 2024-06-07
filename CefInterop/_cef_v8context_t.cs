namespace CefInterop
{
    public unsafe partial struct _cef_v8context_t
    {
        [NativeTypeName("cef_base_ref_counted_t")]
        public _cef_base_ref_counted_t @base;

        [NativeTypeName("struct _cef_task_runner_t *(*)(struct _cef_v8context_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_v8context_t*, _cef_task_runner_t*> get_task_runner;

        [NativeTypeName("int (*)(struct _cef_v8context_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_v8context_t*, int> is_valid;

        [NativeTypeName("struct _cef_browser_t *(*)(struct _cef_v8context_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_v8context_t*, _cef_browser_t*> get_browser;

        [NativeTypeName("struct _cef_frame_t *(*)(struct _cef_v8context_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_v8context_t*, _cef_frame_t*> get_frame;

        [NativeTypeName("struct _cef_v8value_t *(*)(struct _cef_v8context_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_v8context_t*, _cef_v8value_t*> get_global;

        [NativeTypeName("int (*)(struct _cef_v8context_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_v8context_t*, int> enter;

        [NativeTypeName("int (*)(struct _cef_v8context_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_v8context_t*, int> exit;

        [NativeTypeName("int (*)(struct _cef_v8context_t *, struct _cef_v8context_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_v8context_t*, _cef_v8context_t*, int> is_same;

        [NativeTypeName("int (*)(struct _cef_v8context_t *, const cef_string_t *, const cef_string_t *, int, struct _cef_v8value_t **, struct _cef_v8exception_t **) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_v8context_t*, _cef_string_utf16_t*, _cef_string_utf16_t*, int, _cef_v8value_t**, _cef_v8exception_t**, int> eval;
    }
}
