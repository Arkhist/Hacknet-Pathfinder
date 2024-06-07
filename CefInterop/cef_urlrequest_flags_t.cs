namespace CefInterop
{
    public enum cef_urlrequest_flags_t
    {
        UR_FLAG_NONE = 0,
        UR_FLAG_SKIP_CACHE = 1 << 0,
        UR_FLAG_ONLY_FROM_CACHE = 1 << 1,
        UR_FLAG_DISABLE_CACHE = 1 << 2,
        UR_FLAG_ALLOW_STORED_CREDENTIALS = 1 << 3,
        UR_FLAG_REPORT_UPLOAD_PROGRESS = 1 << 4,
        UR_FLAG_NO_DOWNLOAD_DATA = 1 << 5,
        UR_FLAG_NO_RETRY_ON_5XX = 1 << 6,
        UR_FLAG_STOP_ON_REDIRECT = 1 << 7,
    }
}
