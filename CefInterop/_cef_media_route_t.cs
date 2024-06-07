namespace CefInterop
{
    public unsafe partial struct _cef_media_route_t
    {
        [NativeTypeName("cef_base_ref_counted_t")]
        public _cef_base_ref_counted_t @base;

        [NativeTypeName("cef_string_userfree_t (*)(struct _cef_media_route_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_media_route_t*, _cef_string_utf16_t*> get_id;

        [NativeTypeName("struct _cef_media_source_t *(*)(struct _cef_media_route_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_media_route_t*, _cef_media_source_t*> get_source;

        [NativeTypeName("struct _cef_media_sink_t *(*)(struct _cef_media_route_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_media_route_t*, _cef_media_sink_t*> get_sink;

        [NativeTypeName("void (*)(struct _cef_media_route_t *, const void *, size_t) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_media_route_t*, void*, nuint, void> send_route_message;

        [NativeTypeName("void (*)(struct _cef_media_route_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_media_route_t*, void> terminate;
    }
}
