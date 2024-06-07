namespace CefInterop
{
    public enum cef_ssl_content_status_t
    {
        SSL_CONTENT_NORMAL_CONTENT = 0,
        SSL_CONTENT_DISPLAYED_INSECURE_CONTENT = 1 << 0,
        SSL_CONTENT_RAN_INSECURE_CONTENT = 1 << 1,
    }
}
