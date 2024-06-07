namespace CefInterop
{
    public enum cef_context_menu_media_state_flags_t
    {
        CM_MEDIAFLAG_NONE = 0,
        CM_MEDIAFLAG_IN_ERROR = 1 << 0,
        CM_MEDIAFLAG_PAUSED = 1 << 1,
        CM_MEDIAFLAG_MUTED = 1 << 2,
        CM_MEDIAFLAG_LOOP = 1 << 3,
        CM_MEDIAFLAG_CAN_SAVE = 1 << 4,
        CM_MEDIAFLAG_HAS_AUDIO = 1 << 5,
        CM_MEDIAFLAG_CAN_TOGGLE_CONTROLS = 1 << 6,
        CM_MEDIAFLAG_CONTROLS = 1 << 7,
        CM_MEDIAFLAG_CAN_PRINT = 1 << 8,
        CM_MEDIAFLAG_CAN_ROTATE = 1 << 9,
        CM_MEDIAFLAG_CAN_PICTURE_IN_PICTURE = 1 << 10,
        CM_MEDIAFLAG_PICTURE_IN_PICTURE = 1 << 11,
        CM_MEDIAFLAG_CAN_LOOP = 1 << 12,
    }
}
