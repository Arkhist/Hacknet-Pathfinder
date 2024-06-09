using System.Runtime.InteropServices;

namespace CefInterop
{
    public unsafe partial struct _cef_main_args_t_windows
    {
        [NativeTypeName("HINSTANCE")]
        public void* instance;

        [LibraryImport("kernel32")]
        private static partial void* GetModuleHandleA(byte* name);
        
        public void Initialize()
        {
            instance = GetModuleHandleA(null);
        }
    }
}
