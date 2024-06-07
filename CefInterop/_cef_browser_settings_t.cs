namespace CefInterop
{
    public partial struct _cef_browser_settings_t
    {
        [NativeTypeName("size_t")]
        public nuint size;

        public int windowless_frame_rate;

        [NativeTypeName("cef_string_t")]
        public _cef_string_utf16_t standard_font_family;

        [NativeTypeName("cef_string_t")]
        public _cef_string_utf16_t fixed_font_family;

        [NativeTypeName("cef_string_t")]
        public _cef_string_utf16_t serif_font_family;

        [NativeTypeName("cef_string_t")]
        public _cef_string_utf16_t sans_serif_font_family;

        [NativeTypeName("cef_string_t")]
        public _cef_string_utf16_t cursive_font_family;

        [NativeTypeName("cef_string_t")]
        public _cef_string_utf16_t fantasy_font_family;

        public int default_font_size;

        public int default_fixed_font_size;

        public int minimum_font_size;

        public int minimum_logical_font_size;

        [NativeTypeName("cef_string_t")]
        public _cef_string_utf16_t default_encoding;

        public cef_state_t remote_fonts;

        public cef_state_t javascript;

        public cef_state_t javascript_close_windows;

        public cef_state_t javascript_access_clipboard;

        public cef_state_t javascript_dom_paste;

        public cef_state_t image_loading;

        public cef_state_t image_shrink_standalone_to_fit;

        public cef_state_t text_area_resize;

        public cef_state_t tab_to_links;

        public cef_state_t local_storage;

        public cef_state_t databases;

        public cef_state_t webgl;

        [NativeTypeName("cef_color_t")]
        public uint background_color;

        public cef_state_t chrome_status_bubble;

        public cef_state_t chrome_zoom_bubble;
    }
}
