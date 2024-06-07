namespace CefInterop
{
    public enum cef_media_access_permission_types_t
    {
        CEF_MEDIA_PERMISSION_NONE = 0,
        CEF_MEDIA_PERMISSION_DEVICE_AUDIO_CAPTURE = 1 << 0,
        CEF_MEDIA_PERMISSION_DEVICE_VIDEO_CAPTURE = 1 << 1,
        CEF_MEDIA_PERMISSION_DESKTOP_AUDIO_CAPTURE = 1 << 2,
        CEF_MEDIA_PERMISSION_DESKTOP_VIDEO_CAPTURE = 1 << 3,
    }
}
