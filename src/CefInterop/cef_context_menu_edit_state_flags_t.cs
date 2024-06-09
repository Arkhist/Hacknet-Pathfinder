namespace CefInterop
{
    public enum cef_context_menu_edit_state_flags_t
    {
        CM_EDITFLAG_NONE = 0,
        CM_EDITFLAG_CAN_UNDO = 1 << 0,
        CM_EDITFLAG_CAN_REDO = 1 << 1,
        CM_EDITFLAG_CAN_CUT = 1 << 2,
        CM_EDITFLAG_CAN_COPY = 1 << 3,
        CM_EDITFLAG_CAN_PASTE = 1 << 4,
        CM_EDITFLAG_CAN_DELETE = 1 << 5,
        CM_EDITFLAG_CAN_SELECT_ALL = 1 << 6,
        CM_EDITFLAG_CAN_TRANSLATE = 1 << 7,
        CM_EDITFLAG_CAN_EDIT_RICHLY = 1 << 8,
    }
}
