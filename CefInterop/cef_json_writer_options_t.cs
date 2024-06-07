namespace CefInterop
{
    public enum cef_json_writer_options_t
    {
        JSON_WRITER_DEFAULT = 0,
        JSON_WRITER_OMIT_BINARY_VALUES = 1 << 0,
        JSON_WRITER_OMIT_DOUBLE_TYPE_PRESERVATION = 1 << 1,
        JSON_WRITER_PRETTY_PRINT = 1 << 2,
    }
}
