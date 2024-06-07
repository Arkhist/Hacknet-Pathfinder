namespace CefInterop
{
    public enum cef_event_flags_t
    {
        EVENTFLAG_NONE = 0,
        EVENTFLAG_CAPS_LOCK_ON = 1 << 0,
        EVENTFLAG_SHIFT_DOWN = 1 << 1,
        EVENTFLAG_CONTROL_DOWN = 1 << 2,
        EVENTFLAG_ALT_DOWN = 1 << 3,
        EVENTFLAG_LEFT_MOUSE_BUTTON = 1 << 4,
        EVENTFLAG_MIDDLE_MOUSE_BUTTON = 1 << 5,
        EVENTFLAG_RIGHT_MOUSE_BUTTON = 1 << 6,
        EVENTFLAG_COMMAND_DOWN = 1 << 7,
        EVENTFLAG_NUM_LOCK_ON = 1 << 8,
        EVENTFLAG_IS_KEY_PAD = 1 << 9,
        EVENTFLAG_IS_LEFT = 1 << 10,
        EVENTFLAG_IS_RIGHT = 1 << 11,
        EVENTFLAG_ALTGR_DOWN = 1 << 12,
        EVENTFLAG_IS_REPEAT = 1 << 13,
    }
}
