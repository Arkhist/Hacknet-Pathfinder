namespace CefInterop
{
    public unsafe partial struct _cef_resource_bundle_handler_t
    {
        [NativeTypeName("cef_base_ref_counted_t")]
        public _cef_base_ref_counted_t @base;

        [NativeTypeName("int (*)(struct _cef_resource_bundle_handler_t *, int, cef_string_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_resource_bundle_handler_t*, int, _cef_string_utf16_t*, int> get_localized_string;

        [NativeTypeName("int (*)(struct _cef_resource_bundle_handler_t *, int, void **, size_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_resource_bundle_handler_t*, int, void**, nuint*, int> get_data_resource;

        [NativeTypeName("int (*)(struct _cef_resource_bundle_handler_t *, int, cef_scale_factor_t, void **, size_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_resource_bundle_handler_t*, int, cef_scale_factor_t, void**, nuint*, int> get_data_resource_for_scale;
    }
}
