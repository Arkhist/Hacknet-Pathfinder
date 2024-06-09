namespace CefInterop
{
    public unsafe partial struct _cef_x509cert_principal_t
    {
        [NativeTypeName("cef_base_ref_counted_t")]
        public _cef_base_ref_counted_t @base;

        [NativeTypeName("cef_string_userfree_t (*)(struct _cef_x509cert_principal_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_x509cert_principal_t*, _cef_string_utf16_t*> get_display_name;

        [NativeTypeName("cef_string_userfree_t (*)(struct _cef_x509cert_principal_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_x509cert_principal_t*, _cef_string_utf16_t*> get_common_name;

        [NativeTypeName("cef_string_userfree_t (*)(struct _cef_x509cert_principal_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_x509cert_principal_t*, _cef_string_utf16_t*> get_locality_name;

        [NativeTypeName("cef_string_userfree_t (*)(struct _cef_x509cert_principal_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_x509cert_principal_t*, _cef_string_utf16_t*> get_state_or_province_name;

        [NativeTypeName("cef_string_userfree_t (*)(struct _cef_x509cert_principal_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_x509cert_principal_t*, _cef_string_utf16_t*> get_country_name;

        [NativeTypeName("void (*)(struct _cef_x509cert_principal_t *, cef_string_list_t) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_x509cert_principal_t*, _cef_string_list_t*, void> get_organization_names;

        [NativeTypeName("void (*)(struct _cef_x509cert_principal_t *, cef_string_list_t) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_x509cert_principal_t*, _cef_string_list_t*, void> get_organization_unit_names;
    }
}
