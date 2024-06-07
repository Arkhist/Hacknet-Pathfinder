namespace CefInterop
{
    public unsafe partial struct _cef_resource_bundle_t
    {
        [NativeTypeName("cef_base_ref_counted_t")]
        public _cef_base_ref_counted_t @base;

        [NativeTypeName("cef_string_userfree_t (*)(struct _cef_resource_bundle_t *, int) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_resource_bundle_t*, int, _cef_string_utf16_t*> get_localized_string;

        [NativeTypeName("struct _cef_binary_value_t *(*)(struct _cef_resource_bundle_t *, int) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_resource_bundle_t*, int, _cef_binary_value_t*> get_data_resource;

        [NativeTypeName("struct _cef_binary_value_t *(*)(struct _cef_resource_bundle_t *, int, cef_scale_factor_t) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_resource_bundle_t*, int, cef_scale_factor_t, _cef_binary_value_t*> get_data_resource_for_scale;
    }
}
