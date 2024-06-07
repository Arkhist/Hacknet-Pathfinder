namespace CefInterop
{
    public partial struct _cef_composition_underline_t
    {
        [NativeTypeName("cef_range_t")]
        public _cef_range_t range;

        [NativeTypeName("cef_color_t")]
        public uint color;

        [NativeTypeName("cef_color_t")]
        public uint background_color;

        public int thick;

        public cef_composition_underline_style_t style;
    }
}
