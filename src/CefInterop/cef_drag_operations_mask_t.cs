namespace CefInterop
{
    public enum cef_drag_operations_mask_t
    {
        DRAG_OPERATION_NONE = 0,
        DRAG_OPERATION_COPY = 1,
        DRAG_OPERATION_LINK = 2,
        DRAG_OPERATION_GENERIC = 4,
        DRAG_OPERATION_PRIVATE = 8,
        DRAG_OPERATION_MOVE = 16,
        DRAG_OPERATION_DELETE = 32,
        DRAG_OPERATION_EVERY = unchecked((int)(0xffffffff)),
    }
}
