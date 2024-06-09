namespace CefInterop
{
    public enum cef_thread_id_t
    {
        TID_UI,
        TID_FILE_BACKGROUND,
        TID_FILE_USER_VISIBLE,
        TID_FILE_USER_BLOCKING,
        TID_PROCESS_LAUNCHER,
        TID_IO,
        TID_RENDERER,
    }
}
