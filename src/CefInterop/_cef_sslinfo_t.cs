namespace CefInterop
{
    public unsafe partial struct _cef_sslinfo_t
    {
        [NativeTypeName("cef_base_ref_counted_t")]
        public _cef_base_ref_counted_t @base;

        [NativeTypeName("cef_cert_status_t (*)(struct _cef_sslinfo_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_sslinfo_t*, cef_cert_status_t> get_cert_status;

        [NativeTypeName("struct _cef_x509certificate_t *(*)(struct _cef_sslinfo_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_sslinfo_t*, _cef_x509certificate_t*> get_x509certificate;
    }
}
