namespace CefInterop
{
    public unsafe partial struct _cef_frame_handler_t
    {
        [NativeTypeName("cef_base_ref_counted_t")]
        public _cef_base_ref_counted_t @base;

        [NativeTypeName("void (*)(struct _cef_frame_handler_t *, struct _cef_browser_t *, struct _cef_frame_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_frame_handler_t*, _cef_browser_t*, _cef_frame_t*, void> on_frame_created;

        [NativeTypeName("void (*)(struct _cef_frame_handler_t *, struct _cef_browser_t *, struct _cef_frame_t *, int) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_frame_handler_t*, _cef_browser_t*, _cef_frame_t*, int, void> on_frame_attached;

        [NativeTypeName("void (*)(struct _cef_frame_handler_t *, struct _cef_browser_t *, struct _cef_frame_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_frame_handler_t*, _cef_browser_t*, _cef_frame_t*, void> on_frame_detached;

        [NativeTypeName("void (*)(struct _cef_frame_handler_t *, struct _cef_browser_t *, struct _cef_frame_t *, struct _cef_frame_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_frame_handler_t*, _cef_browser_t*, _cef_frame_t*, _cef_frame_t*, void> on_main_frame_changed;
    }
}
