namespace CefInterop
{
    public unsafe partial struct _cef_accelerated_paint_info_t
    {
        [NativeTypeName("HANDLE")]
        public void* shared_texture_handle;

        public cef_color_type_t format;
    }
}
