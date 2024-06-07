namespace CefInterop
{
    public enum cef_context_menu_type_flags_t
    {
        CM_TYPEFLAG_NONE = 0,
        CM_TYPEFLAG_PAGE = 1 << 0,
        CM_TYPEFLAG_FRAME = 1 << 1,
        CM_TYPEFLAG_LINK = 1 << 2,
        CM_TYPEFLAG_MEDIA = 1 << 3,
        CM_TYPEFLAG_SELECTION = 1 << 4,
        CM_TYPEFLAG_EDITABLE = 1 << 5,
    }
}
