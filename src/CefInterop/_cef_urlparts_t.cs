namespace CefInterop
{
    public partial struct _cef_urlparts_t
    {
        [NativeTypeName("cef_string_t")]
        public _cef_string_utf16_t spec;

        [NativeTypeName("cef_string_t")]
        public _cef_string_utf16_t scheme;

        [NativeTypeName("cef_string_t")]
        public _cef_string_utf16_t username;

        [NativeTypeName("cef_string_t")]
        public _cef_string_utf16_t password;

        [NativeTypeName("cef_string_t")]
        public _cef_string_utf16_t host;

        [NativeTypeName("cef_string_t")]
        public _cef_string_utf16_t port;

        [NativeTypeName("cef_string_t")]
        public _cef_string_utf16_t origin;

        [NativeTypeName("cef_string_t")]
        public _cef_string_utf16_t path;

        [NativeTypeName("cef_string_t")]
        public _cef_string_utf16_t query;

        [NativeTypeName("cef_string_t")]
        public _cef_string_utf16_t fragment;
    }
}
