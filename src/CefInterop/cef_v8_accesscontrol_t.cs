namespace CefInterop
{
    public enum cef_v8_accesscontrol_t
    {
        V8_ACCESS_CONTROL_DEFAULT = 0,
        V8_ACCESS_CONTROL_ALL_CAN_READ = 1,
        V8_ACCESS_CONTROL_ALL_CAN_WRITE = 1 << 1,
        V8_ACCESS_CONTROL_PROHIBITS_OVERWRITING = 1 << 2,
    }
}
