namespace CefInterop
{
    public unsafe partial struct _cef_media_router_t
    {
        [NativeTypeName("cef_base_ref_counted_t")]
        public _cef_base_ref_counted_t @base;

        [NativeTypeName("struct _cef_registration_t *(*)(struct _cef_media_router_t *, struct _cef_media_observer_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_media_router_t*, _cef_media_observer_t*, _cef_registration_t*> add_observer;

        [NativeTypeName("struct _cef_media_source_t *(*)(struct _cef_media_router_t *, const cef_string_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_media_router_t*, _cef_string_utf16_t*, _cef_media_source_t*> get_source;

        [NativeTypeName("void (*)(struct _cef_media_router_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_media_router_t*, void> notify_current_sinks;

        [NativeTypeName("void (*)(struct _cef_media_router_t *, struct _cef_media_source_t *, struct _cef_media_sink_t *, struct _cef_media_route_create_callback_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_media_router_t*, _cef_media_source_t*, _cef_media_sink_t*, _cef_media_route_create_callback_t*, void> create_route;

        [NativeTypeName("void (*)(struct _cef_media_router_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_media_router_t*, void> notify_current_routes;
    }
}
