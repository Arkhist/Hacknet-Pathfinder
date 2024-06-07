namespace CefInterop
{
    public partial struct _cef_settings_t
    {
        [NativeTypeName("size_t")]
        public nuint size;

        public int no_sandbox;

        [NativeTypeName("cef_string_t")]
        public _cef_string_utf16_t browser_subprocess_path;

        [NativeTypeName("cef_string_t")]
        public _cef_string_utf16_t framework_dir_path;

        [NativeTypeName("cef_string_t")]
        public _cef_string_utf16_t main_bundle_path;

        public int chrome_runtime;

        public int multi_threaded_message_loop;

        public int external_message_pump;

        public int windowless_rendering_enabled;

        public int command_line_args_disabled;

        [NativeTypeName("cef_string_t")]
        public _cef_string_utf16_t cache_path;

        [NativeTypeName("cef_string_t")]
        public _cef_string_utf16_t root_cache_path;

        public int persist_session_cookies;

        public int persist_user_preferences;

        [NativeTypeName("cef_string_t")]
        public _cef_string_utf16_t user_agent;

        [NativeTypeName("cef_string_t")]
        public _cef_string_utf16_t user_agent_product;

        [NativeTypeName("cef_string_t")]
        public _cef_string_utf16_t locale;

        [NativeTypeName("cef_string_t")]
        public _cef_string_utf16_t log_file;

        public cef_log_severity_t log_severity;

        public cef_log_items_t log_items;

        [NativeTypeName("cef_string_t")]
        public _cef_string_utf16_t javascript_flags;

        [NativeTypeName("cef_string_t")]
        public _cef_string_utf16_t resources_dir_path;

        [NativeTypeName("cef_string_t")]
        public _cef_string_utf16_t locales_dir_path;

        public int pack_loading_disabled;

        public int remote_debugging_port;

        public int uncaught_exception_stack_size;

        [NativeTypeName("cef_color_t")]
        public uint background_color;

        [NativeTypeName("cef_string_t")]
        public _cef_string_utf16_t accept_language_list;

        [NativeTypeName("cef_string_t")]
        public _cef_string_utf16_t cookieable_schemes_list;

        public int cookieable_schemes_exclude_defaults;

        [NativeTypeName("cef_string_t")]
        public _cef_string_utf16_t chrome_policy_id;

        public int chrome_app_icon_id;
    }
}
