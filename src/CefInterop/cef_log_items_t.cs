namespace CefInterop
{
    public enum cef_log_items_t
    {
        LOG_ITEMS_DEFAULT = 0,
        LOG_ITEMS_NONE = 1,
        LOG_ITEMS_FLAG_PROCESS_ID = 1 << 1,
        LOG_ITEMS_FLAG_THREAD_ID = 1 << 2,
        LOG_ITEMS_FLAG_TIME_STAMP = 1 << 3,
        LOG_ITEMS_FLAG_TICK_COUNT = 1 << 4,
    }
}
