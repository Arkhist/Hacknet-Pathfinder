namespace CefInterop
{
    public partial struct _cef_touch_event_t
    {
        public int id;

        public float x;

        public float y;

        public float radius_x;

        public float radius_y;

        public float rotation_angle;

        public float pressure;

        public cef_touch_event_type_t type;

        [NativeTypeName("uint32_t")]
        public uint modifiers;

        public cef_pointer_type_t pointer_type;
    }
}
