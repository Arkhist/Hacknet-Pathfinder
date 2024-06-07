namespace CefInterop
{
    public enum cef_termination_status_t
    {
        TS_ABNORMAL_TERMINATION,
        TS_PROCESS_WAS_KILLED,
        TS_PROCESS_CRASHED,
        TS_PROCESS_OOM,
        TS_LAUNCH_FAILED,
        TS_INTEGRITY_FAILURE,
    }
}
