namespace CefInterop
{
    public unsafe partial struct _cef_scheme_handler_factory_t
    {
        [NativeTypeName("cef_base_ref_counted_t")]
        public _cef_base_ref_counted_t @base;

        [NativeTypeName("struct _cef_resource_handler_t *(*)(struct _cef_scheme_handler_factory_t *, struct _cef_browser_t *, struct _cef_frame_t *, const cef_string_t *, struct _cef_request_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_scheme_handler_factory_t*, _cef_browser_t*, _cef_frame_t*, _cef_string_utf16_t*, _cef_request_t*, _cef_resource_handler_t*> create;
    }
}
