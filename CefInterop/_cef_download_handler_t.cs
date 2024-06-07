namespace CefInterop
{
    public unsafe partial struct _cef_download_handler_t
    {
        [NativeTypeName("cef_base_ref_counted_t")]
        public _cef_base_ref_counted_t @base;

        [NativeTypeName("int (*)(struct _cef_download_handler_t *, struct _cef_browser_t *, const cef_string_t *, const cef_string_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_download_handler_t*, _cef_browser_t*, _cef_string_utf16_t*, _cef_string_utf16_t*, int> can_download;

        [NativeTypeName("int (*)(struct _cef_download_handler_t *, struct _cef_browser_t *, struct _cef_download_item_t *, const cef_string_t *, struct _cef_before_download_callback_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_download_handler_t*, _cef_browser_t*, _cef_download_item_t*, _cef_string_utf16_t*, _cef_before_download_callback_t*, int> on_before_download;

        [NativeTypeName("void (*)(struct _cef_download_handler_t *, struct _cef_browser_t *, struct _cef_download_item_t *, struct _cef_download_item_callback_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_download_handler_t*, _cef_browser_t*, _cef_download_item_t*, _cef_download_item_callback_t*, void> on_download_updated;
    }
}
