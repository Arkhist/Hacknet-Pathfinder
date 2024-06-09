namespace CefInterop
{
    public unsafe partial struct _cef_dev_tools_message_observer_t
    {
        [NativeTypeName("cef_base_ref_counted_t")]
        public _cef_base_ref_counted_t @base;

        [NativeTypeName("int (*)(struct _cef_dev_tools_message_observer_t *, struct _cef_browser_t *, const void *, size_t) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_dev_tools_message_observer_t*, _cef_browser_t*, void*, nuint, int> on_dev_tools_message;

        [NativeTypeName("void (*)(struct _cef_dev_tools_message_observer_t *, struct _cef_browser_t *, int, int, const void *, size_t) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_dev_tools_message_observer_t*, _cef_browser_t*, int, int, void*, nuint, void> on_dev_tools_method_result;

        [NativeTypeName("void (*)(struct _cef_dev_tools_message_observer_t *, struct _cef_browser_t *, const cef_string_t *, const void *, size_t) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_dev_tools_message_observer_t*, _cef_browser_t*, _cef_string_utf16_t*, void*, nuint, void> on_dev_tools_event;

        [NativeTypeName("void (*)(struct _cef_dev_tools_message_observer_t *, struct _cef_browser_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_dev_tools_message_observer_t*, _cef_browser_t*, void> on_dev_tools_agent_attached;

        [NativeTypeName("void (*)(struct _cef_dev_tools_message_observer_t *, struct _cef_browser_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_dev_tools_message_observer_t*, _cef_browser_t*, void> on_dev_tools_agent_detached;
    }
}
