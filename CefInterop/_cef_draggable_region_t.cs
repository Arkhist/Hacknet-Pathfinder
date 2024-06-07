namespace CefInterop
{
    public partial struct _cef_draggable_region_t
    {
        [NativeTypeName("cef_rect_t")]
        public _cef_rect_t bounds;

        public int draggable;
    }
}
