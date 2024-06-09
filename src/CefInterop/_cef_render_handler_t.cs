namespace CefInterop
{
    public unsafe partial struct _cef_render_handler_t
    {
        [NativeTypeName("cef_base_ref_counted_t")]
        public _cef_base_ref_counted_t @base;

        [NativeTypeName("struct _cef_accessibility_handler_t *(*)(struct _cef_render_handler_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_render_handler_t*, _cef_accessibility_handler_t*> get_accessibility_handler;

        [NativeTypeName("int (*)(struct _cef_render_handler_t *, struct _cef_browser_t *, cef_rect_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_render_handler_t*, _cef_browser_t*, _cef_rect_t*, int> get_root_screen_rect;

        [NativeTypeName("void (*)(struct _cef_render_handler_t *, struct _cef_browser_t *, cef_rect_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_render_handler_t*, _cef_browser_t*, _cef_rect_t*, void> get_view_rect;

        [NativeTypeName("int (*)(struct _cef_render_handler_t *, struct _cef_browser_t *, int, int, int *, int *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_render_handler_t*, _cef_browser_t*, int, int, int*, int*, int> get_screen_point;

        [NativeTypeName("int (*)(struct _cef_render_handler_t *, struct _cef_browser_t *, cef_screen_info_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_render_handler_t*, _cef_browser_t*, _cef_screen_info_t*, int> get_screen_info;

        [NativeTypeName("void (*)(struct _cef_render_handler_t *, struct _cef_browser_t *, int) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_render_handler_t*, _cef_browser_t*, int, void> on_popup_show;

        [NativeTypeName("void (*)(struct _cef_render_handler_t *, struct _cef_browser_t *, const cef_rect_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_render_handler_t*, _cef_browser_t*, _cef_rect_t*, void> on_popup_size;

        [NativeTypeName("void (*)(struct _cef_render_handler_t *, struct _cef_browser_t *, cef_paint_element_type_t, size_t, const cef_rect_t *, const void *, int, int) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_render_handler_t*, _cef_browser_t*, cef_paint_element_type_t, nuint, _cef_rect_t*, void*, int, int, void> on_paint;

        [NativeTypeName("void (*)(struct _cef_render_handler_t *, struct _cef_browser_t *, cef_paint_element_type_t, size_t, const cef_rect_t *, const cef_accelerated_paint_info_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_render_handler_t*, _cef_browser_t*, cef_paint_element_type_t, nuint, _cef_rect_t*, _cef_accelerated_paint_info_t*, void> on_accelerated_paint;

        [NativeTypeName("void (*)(struct _cef_render_handler_t *, struct _cef_browser_t *, cef_horizontal_alignment_t, cef_size_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_render_handler_t*, _cef_browser_t*, cef_horizontal_alignment_t, _cef_size_t*, void> get_touch_handle_size;

        [NativeTypeName("void (*)(struct _cef_render_handler_t *, struct _cef_browser_t *, const cef_touch_handle_state_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_render_handler_t*, _cef_browser_t*, _cef_touch_handle_state_t*, void> on_touch_handle_state_changed;

        [NativeTypeName("int (*)(struct _cef_render_handler_t *, struct _cef_browser_t *, struct _cef_drag_data_t *, cef_drag_operations_mask_t, int, int) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_render_handler_t*, _cef_browser_t*, _cef_drag_data_t*, cef_drag_operations_mask_t, int, int, int> start_dragging;

        [NativeTypeName("void (*)(struct _cef_render_handler_t *, struct _cef_browser_t *, cef_drag_operations_mask_t) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_render_handler_t*, _cef_browser_t*, cef_drag_operations_mask_t, void> update_drag_cursor;

        [NativeTypeName("void (*)(struct _cef_render_handler_t *, struct _cef_browser_t *, double, double) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_render_handler_t*, _cef_browser_t*, double, double, void> on_scroll_offset_changed;

        [NativeTypeName("void (*)(struct _cef_render_handler_t *, struct _cef_browser_t *, const cef_range_t *, size_t, const cef_rect_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_render_handler_t*, _cef_browser_t*, _cef_range_t*, nuint, _cef_rect_t*, void> on_ime_composition_range_changed;

        [NativeTypeName("void (*)(struct _cef_render_handler_t *, struct _cef_browser_t *, const cef_string_t *, const cef_range_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_render_handler_t*, _cef_browser_t*, _cef_string_utf16_t*, _cef_range_t*, void> on_text_selection_changed;

        [NativeTypeName("void (*)(struct _cef_render_handler_t *, struct _cef_browser_t *, cef_text_input_mode_t) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_render_handler_t*, _cef_browser_t*, cef_text_input_mode_t, void> on_virtual_keyboard_requested;
    }
}
