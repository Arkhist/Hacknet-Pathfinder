namespace CefInterop
{
    public unsafe partial struct _cef_media_source_t
    {
        [NativeTypeName("cef_base_ref_counted_t")]
        public _cef_base_ref_counted_t @base;

        [NativeTypeName("cef_string_userfree_t (*)(struct _cef_media_source_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_media_source_t*, _cef_string_utf16_t*> get_id;

        [NativeTypeName("int (*)(struct _cef_media_source_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_media_source_t*, int> is_cast_source;

        [NativeTypeName("int (*)(struct _cef_media_source_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_media_source_t*, int> is_dial_source;
    }
}
