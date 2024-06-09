namespace CefInterop
{
    public unsafe partial struct _cef_media_route_create_callback_t
    {
        [NativeTypeName("cef_base_ref_counted_t")]
        public _cef_base_ref_counted_t @base;

        [NativeTypeName("void (*)(struct _cef_media_route_create_callback_t *, cef_media_route_create_result_t, const cef_string_t *, struct _cef_media_route_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_media_route_create_callback_t*, cef_media_route_create_result_t, _cef_string_utf16_t*, _cef_media_route_t*, void> on_media_route_create_finished;
    }
}
