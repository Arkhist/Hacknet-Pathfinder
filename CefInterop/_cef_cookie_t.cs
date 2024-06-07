namespace CefInterop
{
    public partial struct _cef_cookie_t
    {
        [NativeTypeName("cef_string_t")]
        public _cef_string_utf16_t name;

        [NativeTypeName("cef_string_t")]
        public _cef_string_utf16_t value;

        [NativeTypeName("cef_string_t")]
        public _cef_string_utf16_t domain;

        [NativeTypeName("cef_string_t")]
        public _cef_string_utf16_t path;

        public int secure;

        public int httponly;

        [NativeTypeName("cef_basetime_t")]
        public _cef_basetime_t creation;

        [NativeTypeName("cef_basetime_t")]
        public _cef_basetime_t last_access;

        public int has_expires;

        [NativeTypeName("cef_basetime_t")]
        public _cef_basetime_t expires;

        public cef_cookie_same_site_t same_site;

        public cef_cookie_priority_t priority;
    }
}
