namespace CefInterop
{
    public unsafe partial struct _cef_command_handler_t
    {
        [NativeTypeName("cef_base_ref_counted_t")]
        public _cef_base_ref_counted_t @base;

        [NativeTypeName("int (*)(struct _cef_command_handler_t *, struct _cef_browser_t *, int, cef_window_open_disposition_t) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_command_handler_t*, _cef_browser_t*, int, cef_window_open_disposition_t, int> on_chrome_command;

        [NativeTypeName("int (*)(struct _cef_command_handler_t *, struct _cef_browser_t *, int) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_command_handler_t*, _cef_browser_t*, int, int> is_chrome_app_menu_item_visible;

        [NativeTypeName("int (*)(struct _cef_command_handler_t *, struct _cef_browser_t *, int) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_command_handler_t*, _cef_browser_t*, int, int> is_chrome_app_menu_item_enabled;

        [NativeTypeName("int (*)(struct _cef_command_handler_t *, cef_chrome_page_action_icon_type_t) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_command_handler_t*, cef_chrome_page_action_icon_type_t, int> is_chrome_page_action_icon_visible;

        [NativeTypeName("int (*)(struct _cef_command_handler_t *, cef_chrome_toolbar_button_type_t) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_command_handler_t*, cef_chrome_toolbar_button_type_t, int> is_chrome_toolbar_button_visible;
    }
}
