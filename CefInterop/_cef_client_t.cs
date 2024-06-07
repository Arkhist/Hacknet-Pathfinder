namespace CefInterop
{
    public unsafe partial struct _cef_client_t
    {
        [NativeTypeName("cef_base_ref_counted_t")]
        public _cef_base_ref_counted_t @base;

        [NativeTypeName("struct _cef_audio_handler_t *(*)(struct _cef_client_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_client_t*, _cef_audio_handler_t*> get_audio_handler;

        [NativeTypeName("struct _cef_command_handler_t *(*)(struct _cef_client_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_client_t*, _cef_command_handler_t*> get_command_handler;

        [NativeTypeName("struct _cef_context_menu_handler_t *(*)(struct _cef_client_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_client_t*, _cef_context_menu_handler_t*> get_context_menu_handler;

        [NativeTypeName("struct _cef_dialog_handler_t *(*)(struct _cef_client_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_client_t*, _cef_dialog_handler_t*> get_dialog_handler;

        [NativeTypeName("struct _cef_display_handler_t *(*)(struct _cef_client_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_client_t*, _cef_display_handler_t*> get_display_handler;

        [NativeTypeName("struct _cef_download_handler_t *(*)(struct _cef_client_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_client_t*, _cef_download_handler_t*> get_download_handler;

        [NativeTypeName("struct _cef_drag_handler_t *(*)(struct _cef_client_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_client_t*, _cef_drag_handler_t*> get_drag_handler;

        [NativeTypeName("struct _cef_find_handler_t *(*)(struct _cef_client_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_client_t*, _cef_find_handler_t*> get_find_handler;

        [NativeTypeName("struct _cef_focus_handler_t *(*)(struct _cef_client_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_client_t*, _cef_focus_handler_t*> get_focus_handler;

        [NativeTypeName("struct _cef_frame_handler_t *(*)(struct _cef_client_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_client_t*, _cef_frame_handler_t*> get_frame_handler;

        [NativeTypeName("struct _cef_permission_handler_t *(*)(struct _cef_client_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_client_t*, _cef_permission_handler_t*> get_permission_handler;

        [NativeTypeName("struct _cef_jsdialog_handler_t *(*)(struct _cef_client_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_client_t*, _cef_jsdialog_handler_t*> get_jsdialog_handler;

        [NativeTypeName("struct _cef_keyboard_handler_t *(*)(struct _cef_client_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_client_t*, _cef_keyboard_handler_t*> get_keyboard_handler;

        [NativeTypeName("struct _cef_life_span_handler_t *(*)(struct _cef_client_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_client_t*, _cef_life_span_handler_t*> get_life_span_handler;

        [NativeTypeName("struct _cef_load_handler_t *(*)(struct _cef_client_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_client_t*, _cef_load_handler_t*> get_load_handler;

        [NativeTypeName("struct _cef_print_handler_t *(*)(struct _cef_client_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_client_t*, _cef_print_handler_t*> get_print_handler;

        [NativeTypeName("struct _cef_render_handler_t *(*)(struct _cef_client_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_client_t*, _cef_render_handler_t*> get_render_handler;

        [NativeTypeName("struct _cef_request_handler_t *(*)(struct _cef_client_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_client_t*, _cef_request_handler_t*> get_request_handler;

        [NativeTypeName("int (*)(struct _cef_client_t *, struct _cef_browser_t *, struct _cef_frame_t *, cef_process_id_t, struct _cef_process_message_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_client_t*, _cef_browser_t*, _cef_frame_t*, cef_process_id_t, _cef_process_message_t*, int> on_process_message_received;
    }
}
