namespace CefInterop
{
    public unsafe partial struct _cef_string_wide_t
    {
        [NativeTypeName("wchar_t *")]
        public ushort* str;

        [NativeTypeName("size_t")]
        public nuint length;

        [NativeTypeName("void (*)(wchar_t *)")]
        public delegate* unmanaged<ushort*, void> dtor;
    }
}
