namespace CefInterop
{
    public unsafe partial struct _cef_window_info_t_linux
    {
        public _cef_string_utf16_t name;

        public _cef_rect_t bounds;

        public ulong parent_window;

        public int windowless_rendering_enabled;

        public int shared_texturing_enabled;

        public int external_begin_frame_enabled;

        public ulong window;

        public cef_runtime_style_t runtime_style;
    }
}
