namespace CefInterop
{
    public unsafe partial struct _cef_context_menu_handler_t
    {
        [NativeTypeName("cef_base_ref_counted_t")]
        public _cef_base_ref_counted_t @base;

        [NativeTypeName("void (*)(struct _cef_context_menu_handler_t *, struct _cef_browser_t *, struct _cef_frame_t *, struct _cef_context_menu_params_t *, struct _cef_menu_model_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_context_menu_handler_t*, _cef_browser_t*, _cef_frame_t*, _cef_context_menu_params_t*, _cef_menu_model_t*, void> on_before_context_menu;

        [NativeTypeName("int (*)(struct _cef_context_menu_handler_t *, struct _cef_browser_t *, struct _cef_frame_t *, struct _cef_context_menu_params_t *, struct _cef_menu_model_t *, struct _cef_run_context_menu_callback_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_context_menu_handler_t*, _cef_browser_t*, _cef_frame_t*, _cef_context_menu_params_t*, _cef_menu_model_t*, _cef_run_context_menu_callback_t*, int> run_context_menu;

        [NativeTypeName("int (*)(struct _cef_context_menu_handler_t *, struct _cef_browser_t *, struct _cef_frame_t *, struct _cef_context_menu_params_t *, int, cef_event_flags_t) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_context_menu_handler_t*, _cef_browser_t*, _cef_frame_t*, _cef_context_menu_params_t*, int, cef_event_flags_t, int> on_context_menu_command;

        [NativeTypeName("void (*)(struct _cef_context_menu_handler_t *, struct _cef_browser_t *, struct _cef_frame_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_context_menu_handler_t*, _cef_browser_t*, _cef_frame_t*, void> on_context_menu_dismissed;

        [NativeTypeName("int (*)(struct _cef_context_menu_handler_t *, struct _cef_browser_t *, struct _cef_frame_t *, const cef_point_t *, const cef_size_t *, cef_quick_menu_edit_state_flags_t, struct _cef_run_quick_menu_callback_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_context_menu_handler_t*, _cef_browser_t*, _cef_frame_t*, _cef_point_t*, _cef_size_t*, cef_quick_menu_edit_state_flags_t, _cef_run_quick_menu_callback_t*, int> run_quick_menu;

        [NativeTypeName("int (*)(struct _cef_context_menu_handler_t *, struct _cef_browser_t *, struct _cef_frame_t *, int, cef_event_flags_t) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_context_menu_handler_t*, _cef_browser_t*, _cef_frame_t*, int, cef_event_flags_t, int> on_quick_menu_command;

        [NativeTypeName("void (*)(struct _cef_context_menu_handler_t *, struct _cef_browser_t *, struct _cef_frame_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_context_menu_handler_t*, _cef_browser_t*, _cef_frame_t*, void> on_quick_menu_dismissed;
    }
}
