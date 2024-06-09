namespace CefInterop
{
    public unsafe partial struct _cef_download_image_callback_t
    {
        [NativeTypeName("cef_base_ref_counted_t")]
        public _cef_base_ref_counted_t @base;

        [NativeTypeName("void (*)(struct _cef_download_image_callback_t *, const cef_string_t *, int, struct _cef_image_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_download_image_callback_t*, _cef_string_utf16_t*, int, _cef_image_t*, void> on_download_image_finished;
    }
}
