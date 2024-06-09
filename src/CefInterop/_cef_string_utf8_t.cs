namespace CefInterop
{
    public unsafe partial struct _cef_string_utf8_t
    {
        [NativeTypeName("char *")]
        public sbyte* str;

        [NativeTypeName("size_t")]
        public nuint length;

        [NativeTypeName("void (*)(char *)")]
        public delegate* unmanaged<sbyte*, void> dtor;
    }
}
