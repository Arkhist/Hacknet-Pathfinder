namespace CefInterop
{
    public unsafe partial struct _cef_read_handler_t
    {
        [NativeTypeName("cef_base_ref_counted_t")]
        public _cef_base_ref_counted_t @base;

        [NativeTypeName("size_t (*)(struct _cef_read_handler_t *, void *, size_t, size_t) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_read_handler_t*, void*, nuint, nuint, nuint> read;

        [NativeTypeName("int (*)(struct _cef_read_handler_t *, int64_t, int) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_read_handler_t*, long, int, int> seek;

        [NativeTypeName("int64_t (*)(struct _cef_read_handler_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_read_handler_t*, long> tell;

        [NativeTypeName("int (*)(struct _cef_read_handler_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_read_handler_t*, int> eof;

        [NativeTypeName("int (*)(struct _cef_read_handler_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_read_handler_t*, int> may_block;
    }
}
