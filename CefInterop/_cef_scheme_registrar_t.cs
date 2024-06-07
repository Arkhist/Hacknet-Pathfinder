namespace CefInterop
{
    public unsafe partial struct _cef_scheme_registrar_t
    {
        [NativeTypeName("cef_base_scoped_t")]
        public _cef_base_scoped_t @base;

        [NativeTypeName("int (*)(struct _cef_scheme_registrar_t *, const cef_string_t *, int) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_scheme_registrar_t*, _cef_string_utf16_t*, int, int> add_custom_scheme;
    }
}
