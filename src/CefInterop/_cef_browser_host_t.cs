namespace CefInterop
{
    public unsafe partial struct _cef_browser_host_t
    {
        [NativeTypeName("cef_base_ref_counted_t")]
        public _cef_base_ref_counted_t @base;

        [NativeTypeName("struct _cef_browser_t *(*)(struct _cef_browser_host_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_host_t*, _cef_browser_t*> get_browser;

        [NativeTypeName("void (*)(struct _cef_browser_host_t *, int) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_host_t*, int, void> close_browser;

        [NativeTypeName("int (*)(struct _cef_browser_host_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_host_t*, int> try_close_browser;

        [NativeTypeName("void (*)(struct _cef_browser_host_t *, int) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_host_t*, int, void> set_focus;

        [NativeTypeName("HWND (*)(struct _cef_browser_host_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_host_t*, void*> get_window_handle;

        [NativeTypeName("HWND (*)(struct _cef_browser_host_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_host_t*, void*> get_opener_window_handle;

        [NativeTypeName("int (*)(struct _cef_browser_host_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_host_t*, int> has_view;

        [NativeTypeName("struct _cef_client_t *(*)(struct _cef_browser_host_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_host_t*, _cef_client_t*> get_client;

        [NativeTypeName("struct _cef_request_context_t *(*)(struct _cef_browser_host_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_host_t*, _cef_request_context_t*> get_request_context;

        [NativeTypeName("int (*)(struct _cef_browser_host_t *, cef_zoom_command_t) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_host_t*, cef_zoom_command_t, int> can_zoom;

        [NativeTypeName("void (*)(struct _cef_browser_host_t *, cef_zoom_command_t) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_host_t*, cef_zoom_command_t, void> zoom;

        [NativeTypeName("double (*)(struct _cef_browser_host_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_host_t*, double> get_default_zoom_level;

        [NativeTypeName("double (*)(struct _cef_browser_host_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_host_t*, double> get_zoom_level;

        [NativeTypeName("void (*)(struct _cef_browser_host_t *, double) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_host_t*, double, void> set_zoom_level;

        [NativeTypeName("void (*)(struct _cef_browser_host_t *, cef_file_dialog_mode_t, const cef_string_t *, const cef_string_t *, cef_string_list_t, struct _cef_run_file_dialog_callback_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_host_t*, cef_file_dialog_mode_t, _cef_string_utf16_t*, _cef_string_utf16_t*, _cef_string_list_t*, _cef_run_file_dialog_callback_t*, void> run_file_dialog;

        [NativeTypeName("void (*)(struct _cef_browser_host_t *, const cef_string_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_host_t*, _cef_string_utf16_t*, void> start_download;

        [NativeTypeName("void (*)(struct _cef_browser_host_t *, const cef_string_t *, int, uint32_t, int, struct _cef_download_image_callback_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_host_t*, _cef_string_utf16_t*, int, uint, int, _cef_download_image_callback_t*, void> download_image;

        [NativeTypeName("void (*)(struct _cef_browser_host_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_host_t*, void> print;

        [NativeTypeName("void (*)(struct _cef_browser_host_t *, const cef_string_t *, const struct _cef_pdf_print_settings_t *, struct _cef_pdf_print_callback_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_host_t*, _cef_string_utf16_t*, _cef_pdf_print_settings_t*, _cef_pdf_print_callback_t*, void> print_to_pdf;

        [NativeTypeName("void (*)(struct _cef_browser_host_t *, const cef_string_t *, int, int, int) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_host_t*, _cef_string_utf16_t*, int, int, int, void> find;

        [NativeTypeName("void (*)(struct _cef_browser_host_t *, int) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_host_t*, int, void> stop_finding;

        [NativeTypeName("void (*)(struct _cef_browser_host_t *, const struct _cef_window_info_t *, struct _cef_client_t *, const struct _cef_browser_settings_t *, const cef_point_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_host_t*, void*, _cef_client_t*, _cef_browser_settings_t*, _cef_point_t*, void> show_dev_tools;

        [NativeTypeName("void (*)(struct _cef_browser_host_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_host_t*, void> close_dev_tools;

        [NativeTypeName("int (*)(struct _cef_browser_host_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_host_t*, int> has_dev_tools;

        [NativeTypeName("int (*)(struct _cef_browser_host_t *, const void *, size_t) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_host_t*, void*, nuint, int> send_dev_tools_message;

        [NativeTypeName("int (*)(struct _cef_browser_host_t *, int, const cef_string_t *, struct _cef_dictionary_value_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_host_t*, int, _cef_string_utf16_t*, _cef_dictionary_value_t*, int> execute_dev_tools_method;

        [NativeTypeName("struct _cef_registration_t *(*)(struct _cef_browser_host_t *, struct _cef_dev_tools_message_observer_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_host_t*, _cef_dev_tools_message_observer_t*, _cef_registration_t*> add_dev_tools_message_observer;

        [NativeTypeName("void (*)(struct _cef_browser_host_t *, struct _cef_navigation_entry_visitor_t *, int) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_host_t*, _cef_navigation_entry_visitor_t*, int, void> get_navigation_entries;

        [NativeTypeName("void (*)(struct _cef_browser_host_t *, const cef_string_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_host_t*, _cef_string_utf16_t*, void> replace_misspelling;

        [NativeTypeName("void (*)(struct _cef_browser_host_t *, const cef_string_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_host_t*, _cef_string_utf16_t*, void> add_word_to_dictionary;

        [NativeTypeName("int (*)(struct _cef_browser_host_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_host_t*, int> is_window_rendering_disabled;

        [NativeTypeName("void (*)(struct _cef_browser_host_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_host_t*, void> was_resized;

        [NativeTypeName("void (*)(struct _cef_browser_host_t *, int) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_host_t*, int, void> was_hidden;

        [NativeTypeName("void (*)(struct _cef_browser_host_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_host_t*, void> notify_screen_info_changed;

        [NativeTypeName("void (*)(struct _cef_browser_host_t *, cef_paint_element_type_t) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_host_t*, cef_paint_element_type_t, void> invalidate;

        [NativeTypeName("void (*)(struct _cef_browser_host_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_host_t*, void> send_external_begin_frame;

        [NativeTypeName("void (*)(struct _cef_browser_host_t *, const cef_key_event_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_host_t*, _cef_key_event_t*, void> send_key_event;

        [NativeTypeName("void (*)(struct _cef_browser_host_t *, const cef_mouse_event_t *, cef_mouse_button_type_t, int, int) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_host_t*, _cef_mouse_event_t*, cef_mouse_button_type_t, int, int, void> send_mouse_click_event;

        [NativeTypeName("void (*)(struct _cef_browser_host_t *, const cef_mouse_event_t *, int) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_host_t*, _cef_mouse_event_t*, int, void> send_mouse_move_event;

        [NativeTypeName("void (*)(struct _cef_browser_host_t *, const cef_mouse_event_t *, int, int) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_host_t*, _cef_mouse_event_t*, int, int, void> send_mouse_wheel_event;

        [NativeTypeName("void (*)(struct _cef_browser_host_t *, const cef_touch_event_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_host_t*, _cef_touch_event_t*, void> send_touch_event;

        [NativeTypeName("void (*)(struct _cef_browser_host_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_host_t*, void> send_capture_lost_event;

        [NativeTypeName("void (*)(struct _cef_browser_host_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_host_t*, void> notify_move_or_resize_started;

        [NativeTypeName("int (*)(struct _cef_browser_host_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_host_t*, int> get_windowless_frame_rate;

        [NativeTypeName("void (*)(struct _cef_browser_host_t *, int) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_host_t*, int, void> set_windowless_frame_rate;

        [NativeTypeName("void (*)(struct _cef_browser_host_t *, const cef_string_t *, size_t, const cef_composition_underline_t *, const cef_range_t *, const cef_range_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_host_t*, _cef_string_utf16_t*, nuint, _cef_composition_underline_t*, _cef_range_t*, _cef_range_t*, void> ime_set_composition;

        [NativeTypeName("void (*)(struct _cef_browser_host_t *, const cef_string_t *, const cef_range_t *, int) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_host_t*, _cef_string_utf16_t*, _cef_range_t*, int, void> ime_commit_text;

        [NativeTypeName("void (*)(struct _cef_browser_host_t *, int) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_host_t*, int, void> ime_finish_composing_text;

        [NativeTypeName("void (*)(struct _cef_browser_host_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_host_t*, void> ime_cancel_composition;

        [NativeTypeName("void (*)(struct _cef_browser_host_t *, struct _cef_drag_data_t *, const cef_mouse_event_t *, cef_drag_operations_mask_t) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_host_t*, _cef_drag_data_t*, _cef_mouse_event_t*, cef_drag_operations_mask_t, void> drag_target_drag_enter;

        [NativeTypeName("void (*)(struct _cef_browser_host_t *, const cef_mouse_event_t *, cef_drag_operations_mask_t) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_host_t*, _cef_mouse_event_t*, cef_drag_operations_mask_t, void> drag_target_drag_over;

        [NativeTypeName("void (*)(struct _cef_browser_host_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_host_t*, void> drag_target_drag_leave;

        [NativeTypeName("void (*)(struct _cef_browser_host_t *, const cef_mouse_event_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_host_t*, _cef_mouse_event_t*, void> drag_target_drop;

        [NativeTypeName("void (*)(struct _cef_browser_host_t *, int, int, cef_drag_operations_mask_t) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_host_t*, int, int, cef_drag_operations_mask_t, void> drag_source_ended_at;

        [NativeTypeName("void (*)(struct _cef_browser_host_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_host_t*, void> drag_source_system_drag_ended;

        [NativeTypeName("struct _cef_navigation_entry_t *(*)(struct _cef_browser_host_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_host_t*, _cef_navigation_entry_t*> get_visible_navigation_entry;

        [NativeTypeName("void (*)(struct _cef_browser_host_t *, cef_state_t) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_host_t*, cef_state_t, void> set_accessibility_state;

        [NativeTypeName("void (*)(struct _cef_browser_host_t *, int, const cef_size_t *, const cef_size_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_host_t*, int, _cef_size_t*, _cef_size_t*, void> set_auto_resize_enabled;

        [NativeTypeName("struct _cef_extension_t *(*)(struct _cef_browser_host_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_host_t*, _cef_extension_t*> get_extension;

        [NativeTypeName("int (*)(struct _cef_browser_host_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_host_t*, int> is_background_host;

        [NativeTypeName("void (*)(struct _cef_browser_host_t *, int) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_host_t*, int, void> set_audio_muted;

        [NativeTypeName("int (*)(struct _cef_browser_host_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_host_t*, int> is_audio_muted;

        [NativeTypeName("int (*)(struct _cef_browser_host_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_host_t*, int> is_fullscreen;

        [NativeTypeName("void (*)(struct _cef_browser_host_t *, int) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_host_t*, int, void> exit_fullscreen;

        [NativeTypeName("int (*)(struct _cef_browser_host_t *, int) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_host_t*, int, int> can_execute_chrome_command;

        [NativeTypeName("void (*)(struct _cef_browser_host_t *, int, cef_window_open_disposition_t) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_host_t*, int, cef_window_open_disposition_t, void> execute_chrome_command;

        [NativeTypeName("int (*)(struct _cef_browser_host_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_host_t*, int> is_render_process_unresponsive;

        [NativeTypeName("cef_runtime_style_t (*)(struct _cef_browser_host_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_browser_host_t*, cef_runtime_style_t> get_runtime_style;
    }
}
