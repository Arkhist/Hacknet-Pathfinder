namespace CefInterop
{
    public unsafe partial struct _cef_sslstatus_t
    {
        [NativeTypeName("cef_base_ref_counted_t")]
        public _cef_base_ref_counted_t @base;

        [NativeTypeName("int (*)(struct _cef_sslstatus_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_sslstatus_t*, int> is_secure_connection;

        [NativeTypeName("cef_cert_status_t (*)(struct _cef_sslstatus_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_sslstatus_t*, cef_cert_status_t> get_cert_status;

        [NativeTypeName("cef_ssl_version_t (*)(struct _cef_sslstatus_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_sslstatus_t*, cef_ssl_version_t> get_sslversion;

        [NativeTypeName("cef_ssl_content_status_t (*)(struct _cef_sslstatus_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_sslstatus_t*, cef_ssl_content_status_t> get_content_status;

        [NativeTypeName("struct _cef_x509certificate_t *(*)(struct _cef_sslstatus_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_sslstatus_t*, _cef_x509certificate_t*> get_x509certificate;
    }
}
