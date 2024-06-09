namespace CefInterop
{
    public unsafe partial struct _cef_drag_handler_t
    {
        [NativeTypeName("cef_base_ref_counted_t")]
        public _cef_base_ref_counted_t @base;

        [NativeTypeName("int (*)(struct _cef_drag_handler_t *, struct _cef_browser_t *, struct _cef_drag_data_t *, cef_drag_operations_mask_t) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_drag_handler_t*, _cef_browser_t*, _cef_drag_data_t*, cef_drag_operations_mask_t, int> on_drag_enter;

        [NativeTypeName("void (*)(struct _cef_drag_handler_t *, struct _cef_browser_t *, struct _cef_frame_t *, size_t, const cef_draggable_region_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_drag_handler_t*, _cef_browser_t*, _cef_frame_t*, nuint, _cef_draggable_region_t*, void> on_draggable_regions_changed;
    }
}
