namespace CefInterop
{
    public enum cef_uri_unescape_rule_t
    {
        UU_NONE = 0,
        UU_NORMAL = 1 << 0,
        UU_SPACES = 1 << 1,
        UU_PATH_SEPARATORS = 1 << 2,
        UU_URL_SPECIAL_CHARS_EXCEPT_PATH_SEPARATORS = 1 << 3,
        UU_REPLACE_PLUS_WITH_SPACE = 1 << 4,
    }
}
