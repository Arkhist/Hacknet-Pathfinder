namespace CefInterop
{
    public partial struct _cef_box_layout_settings_t
    {
        public int horizontal;

        public int inside_border_horizontal_spacing;

        public int inside_border_vertical_spacing;

        [NativeTypeName("cef_insets_t")]
        public _cef_insets_t inside_border_insets;

        public int between_child_spacing;

        public cef_axis_alignment_t main_axis_alignment;

        public cef_axis_alignment_t cross_axis_alignment;

        public int minimum_cross_axis_size;

        public int default_flex;
    }
}
