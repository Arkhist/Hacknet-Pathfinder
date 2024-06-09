namespace CefInterop
{
    public unsafe partial struct _cef_response_filter_t
    {
        [NativeTypeName("cef_base_ref_counted_t")]
        public _cef_base_ref_counted_t @base;

        [NativeTypeName("int (*)(struct _cef_response_filter_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_response_filter_t*, int> init_filter;

        [NativeTypeName("cef_response_filter_status_t (*)(struct _cef_response_filter_t *, void *, size_t, size_t *, void *, size_t, size_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_response_filter_t*, void*, nuint, nuint*, void*, nuint, nuint*, cef_response_filter_status_t> filter;
    }
}
