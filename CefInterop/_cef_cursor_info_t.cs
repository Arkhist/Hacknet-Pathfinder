namespace CefInterop
{
    public unsafe partial struct _cef_cursor_info_t
    {
        [NativeTypeName("cef_point_t")]
        public _cef_point_t hotspot;

        public float image_scale_factor;

        public void* buffer;

        [NativeTypeName("cef_size_t")]
        public _cef_size_t size;
    }
}
