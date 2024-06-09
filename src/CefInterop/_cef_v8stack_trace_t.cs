namespace CefInterop
{
    public unsafe partial struct _cef_v8stack_trace_t
    {
        [NativeTypeName("cef_base_ref_counted_t")]
        public _cef_base_ref_counted_t @base;

        [NativeTypeName("int (*)(struct _cef_v8stack_trace_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_v8stack_trace_t*, int> is_valid;

        [NativeTypeName("int (*)(struct _cef_v8stack_trace_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_v8stack_trace_t*, int> get_frame_count;

        [NativeTypeName("struct _cef_v8stack_frame_t *(*)(struct _cef_v8stack_trace_t *, int) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_v8stack_trace_t*, int, _cef_v8stack_frame_t*> get_frame;
    }
}
