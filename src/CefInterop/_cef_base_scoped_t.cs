namespace CefInterop
{
    public unsafe partial struct _cef_base_scoped_t
    {
        [NativeTypeName("size_t")]
        public nuint size;

        [NativeTypeName("void (*)(struct _cef_base_scoped_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_base_scoped_t*, void> del;
    }
}
