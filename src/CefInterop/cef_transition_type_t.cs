namespace CefInterop
{
    public enum cef_transition_type_t
    {
        TT_LINK = 0,
        TT_EXPLICIT = 1,
        TT_AUTO_BOOKMARK = 2,
        TT_AUTO_SUBFRAME = 3,
        TT_MANUAL_SUBFRAME = 4,
        TT_GENERATED = 5,
        TT_AUTO_TOPLEVEL = 6,
        TT_FORM_SUBMIT = 7,
        TT_RELOAD = 8,
        TT_KEYWORD = 9,
        TT_KEYWORD_GENERATED = 10,
        TT_SOURCE_MASK = 0xFF,
        TT_BLOCKED_FLAG = 0x00800000,
        TT_FORWARD_BACK_FLAG = 0x01000000,
        TT_DIRECT_LOAD_FLAG = 0x02000000,
        TT_HOME_PAGE_FLAG = 0x04000000,
        TT_FROM_API_FLAG = 0x08000000,
        TT_CHAIN_START_FLAG = 0x10000000,
        TT_CHAIN_END_FLAG = 0x20000000,
        TT_CLIENT_REDIRECT_FLAG = 0x40000000,
        TT_SERVER_REDIRECT_FLAG = unchecked((int)(0x80000000)),
        TT_IS_REDIRECT_MASK = unchecked((int)(0xC0000000)),
        TT_QUALIFIER_MASK = unchecked((int)(0xFFFFFF00)),
    }
}
