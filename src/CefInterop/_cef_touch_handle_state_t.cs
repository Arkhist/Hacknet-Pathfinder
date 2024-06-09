namespace CefInterop
{
    public partial struct _cef_touch_handle_state_t
    {
        public int touch_handle_id;

        [NativeTypeName("uint32_t")]
        public uint flags;

        public int enabled;

        public cef_horizontal_alignment_t orientation;

        public int mirror_vertical;

        public int mirror_horizontal;

        [NativeTypeName("cef_point_t")]
        public _cef_point_t origin;

        public float alpha;
    }
}
