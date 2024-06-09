namespace CefInterop
{
    public unsafe partial struct _cef_select_client_certificate_callback_t
    {
        [NativeTypeName("cef_base_ref_counted_t")]
        public _cef_base_ref_counted_t @base;

        [NativeTypeName("void (*)(struct _cef_select_client_certificate_callback_t *, struct _cef_x509certificate_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_select_client_certificate_callback_t*, _cef_x509certificate_t*, void> select;
    }
}
