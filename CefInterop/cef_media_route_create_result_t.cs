namespace CefInterop
{
    public enum cef_media_route_create_result_t
    {
        CEF_MRCR_UNKNOWN_ERROR = 0,
        CEF_MRCR_OK = 1,
        CEF_MRCR_TIMED_OUT = 2,
        CEF_MRCR_ROUTE_NOT_FOUND = 3,
        CEF_MRCR_SINK_NOT_FOUND = 4,
        CEF_MRCR_INVALID_ORIGIN = 5,
        CEF_MRCR_NO_SUPPORTED_PROVIDER = 7,
        CEF_MRCR_CANCELLED = 8,
        CEF_MRCR_ROUTE_ALREADY_EXISTS = 9,
        CEF_MRCR_ROUTE_ALREADY_TERMINATED = 11,
    }
}
