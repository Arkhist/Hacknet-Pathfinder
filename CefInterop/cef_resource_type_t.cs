namespace CefInterop
{
    public enum cef_resource_type_t
    {
        RT_MAIN_FRAME = 0,
        RT_SUB_FRAME,
        RT_STYLESHEET,
        RT_SCRIPT,
        RT_IMAGE,
        RT_FONT_RESOURCE,
        RT_SUB_RESOURCE,
        RT_OBJECT,
        RT_MEDIA,
        RT_WORKER,
        RT_SHARED_WORKER,
        RT_PREFETCH,
        RT_FAVICON,
        RT_XHR,
        RT_PING,
        RT_SERVICE_WORKER,
        RT_CSP_REPORT,
        RT_PLUGIN_RESOURCE,
        RT_NAVIGATION_PRELOAD_MAIN_FRAME = 19,
        RT_NAVIGATION_PRELOAD_SUB_FRAME,
    }
}
