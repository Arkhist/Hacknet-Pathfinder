namespace CefInterop
{
    public unsafe partial struct _cef_browser_process_handler_t
    {
        [NativeTypeName("cef_base_ref_counted_t")]
        public _cef_base_ref_counted_t @base;

        [NativeTypeName("void (*)(struct _cef_browser_process_handler_t *, cef_preferences_type_t, struct _cef_preference_registrar_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_process_handler_t*, cef_preferences_type_t, _cef_preference_registrar_t*, void> on_register_custom_preferences;

        [NativeTypeName("void (*)(struct _cef_browser_process_handler_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_process_handler_t*, void> on_context_initialized;

        [NativeTypeName("void (*)(struct _cef_browser_process_handler_t *, struct _cef_command_line_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_process_handler_t*, _cef_command_line_t*, void> on_before_child_process_launch;

        [NativeTypeName("int (*)(struct _cef_browser_process_handler_t *, struct _cef_command_line_t *, const cef_string_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_process_handler_t*, _cef_command_line_t*, _cef_string_utf16_t*, int> on_already_running_app_relaunch;

        [NativeTypeName("void (*)(struct _cef_browser_process_handler_t *, int64_t) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_process_handler_t*, long, void> on_schedule_message_pump_work;

        [NativeTypeName("struct _cef_client_t *(*)(struct _cef_browser_process_handler_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_process_handler_t*, _cef_client_t*> get_default_client;

        [NativeTypeName("struct _cef_request_context_handler_t *(*)(struct _cef_browser_process_handler_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_process_handler_t*, _cef_request_context_handler_t*> get_default_request_context_handler;
    }
}
