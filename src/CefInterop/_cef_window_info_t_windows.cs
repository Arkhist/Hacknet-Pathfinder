namespace CefInterop
{
    public unsafe partial struct _cef_window_info_t_windows
    {
        [NativeTypeName("DWORD")]
        public uint ex_style;

        [NativeTypeName("cef_string_t")]
        public _cef_string_utf16_t window_name;

        [NativeTypeName("DWORD")]
        public uint style;

        [NativeTypeName("cef_rect_t")]
        public _cef_rect_t bounds;

        [NativeTypeName("HWND")]
        public void* parent_window;

        [NativeTypeName("HMENU")]
        public void* menu;

        public int windowless_rendering_enabled;

        public int shared_texture_enabled;

        public int external_begin_frame_enabled;

        [NativeTypeName("HWND")]
        public void* window;

        public cef_runtime_style_t runtime_style;
    }
}
