namespace CefInterop
{
    public enum cef_ssl_version_t
    {
        SSL_CONNECTION_VERSION_UNKNOWN = 0,
        SSL_CONNECTION_VERSION_SSL2 = 1,
        SSL_CONNECTION_VERSION_SSL3 = 2,
        SSL_CONNECTION_VERSION_TLS1 = 3,
        SSL_CONNECTION_VERSION_TLS1_1 = 4,
        SSL_CONNECTION_VERSION_TLS1_2 = 5,
        SSL_CONNECTION_VERSION_TLS1_3 = 6,
        SSL_CONNECTION_VERSION_QUIC = 7,
    }
}
