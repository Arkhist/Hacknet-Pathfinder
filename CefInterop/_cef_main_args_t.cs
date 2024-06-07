namespace CefInterop
{
    public unsafe partial struct _cef_main_args_t
    {
        [NativeTypeName("HINSTANCE")]
        public void* instance;
    }
}
