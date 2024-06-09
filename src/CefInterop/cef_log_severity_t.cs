namespace CefInterop
{
    public enum cef_log_severity_t
    {
        LOGSEVERITY_DEFAULT,
        LOGSEVERITY_VERBOSE,
        LOGSEVERITY_DEBUG = LOGSEVERITY_VERBOSE,
        LOGSEVERITY_INFO,
        LOGSEVERITY_WARNING,
        LOGSEVERITY_ERROR,
        LOGSEVERITY_FATAL,
        LOGSEVERITY_DISABLE = 99,
    }
}
