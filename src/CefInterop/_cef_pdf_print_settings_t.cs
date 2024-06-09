namespace CefInterop
{
    public partial struct _cef_pdf_print_settings_t
    {
        public int landscape;

        public int print_background;

        public double scale;

        public double paper_width;

        public double paper_height;

        public int prefer_css_page_size;

        public cef_pdf_print_margin_type_t margin_type;

        public double margin_top;

        public double margin_right;

        public double margin_bottom;

        public double margin_left;

        [NativeTypeName("cef_string_t")]
        public _cef_string_utf16_t page_ranges;

        public int display_header_footer;

        [NativeTypeName("cef_string_t")]
        public _cef_string_utf16_t header_template;

        [NativeTypeName("cef_string_t")]
        public _cef_string_utf16_t footer_template;

        public int generate_tagged_pdf;

        public int generate_document_outline;
    }
}
