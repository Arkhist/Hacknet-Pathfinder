namespace CefInterop
{
    public partial struct _cef_mouse_event_t
    {
        public int x;

        public int y;

        [NativeTypeName("uint32_t")]
        public uint modifiers;
    }
}
