using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace CefInterop
{
    public unsafe partial struct _cef_main_args_t_linux : IDisposable
    {
        public int argc;
        public byte** argv;
        
        public void Initialize(string mainProcessPath)
        {
            var args = Environment.GetCommandLineArgs();
            argc = args.Length + 1;
            argv = (byte**)NativeMemory.Alloc((nuint)sizeof(byte**) * (nuint)argc);
            argv[0] = Utf8StringMarshaller.ConvertToUnmanaged(mainProcessPath);
            for (int i = 0; i < args.Length; i++)
            {
                argv[i + 1] = Utf8StringMarshaller.ConvertToUnmanaged(args[i]);
            }
        }

        public void Dispose()
        {
            if (argv != null)
            {
                for (int i = 0; i < argc; i++)
                {
                    Utf8StringMarshaller.Free(argv[i]);
                }
                NativeMemory.Free(argv);
                this = default;
            }
        }
    }
}
