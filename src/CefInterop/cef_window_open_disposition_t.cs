namespace CefInterop
{
    public enum cef_window_open_disposition_t
    {
        CEF_WOD_UNKNOWN,
        CEF_WOD_CURRENT_TAB,
        CEF_WOD_SINGLETON_TAB,
        CEF_WOD_NEW_FOREGROUND_TAB,
        CEF_WOD_NEW_BACKGROUND_TAB,
        CEF_WOD_NEW_POPUP,
        CEF_WOD_NEW_WINDOW,
        CEF_WOD_SAVE_TO_DISK,
        CEF_WOD_OFF_THE_RECORD,
        CEF_WOD_IGNORE_ACTION,
        CEF_WOD_SWITCH_TO_TAB,
        CEF_WOD_NEW_PICTURE_IN_PICTURE,
        CEF_WOD_MAX_VALUE = CEF_WOD_NEW_PICTURE_IN_PICTURE,
    }
}
