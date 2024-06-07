namespace CefInterop
{
    public partial struct _cef_key_event_t
    {
        public cef_key_event_type_t type;

        [NativeTypeName("uint32_t")]
        public uint modifiers;

        public int windows_key_code;

        public int native_key_code;

        public int is_system_key;

        [NativeTypeName("char16_t")]
        public ushort character;

        [NativeTypeName("char16_t")]
        public ushort unmodified_character;

        public int focus_on_editable_field;
    }
}
