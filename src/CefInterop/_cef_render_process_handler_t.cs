namespace CefInterop
{
    public unsafe partial struct _cef_render_process_handler_t
    {
        [NativeTypeName("cef_base_ref_counted_t")]
        public _cef_base_ref_counted_t @base;

        [NativeTypeName("void (*)(struct _cef_render_process_handler_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_render_process_handler_t*, void> on_web_kit_initialized;

        [NativeTypeName("void (*)(struct _cef_render_process_handler_t *, struct _cef_browser_t *, struct _cef_dictionary_value_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_render_process_handler_t*, _cef_browser_t*, _cef_dictionary_value_t*, void> on_browser_created;

        [NativeTypeName("void (*)(struct _cef_render_process_handler_t *, struct _cef_browser_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_render_process_handler_t*, _cef_browser_t*, void> on_browser_destroyed;

        [NativeTypeName("struct _cef_load_handler_t *(*)(struct _cef_render_process_handler_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_render_process_handler_t*, _cef_load_handler_t*> get_load_handler;

        [NativeTypeName("void (*)(struct _cef_render_process_handler_t *, struct _cef_browser_t *, struct _cef_frame_t *, struct _cef_v8context_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_render_process_handler_t*, _cef_browser_t*, _cef_frame_t*, _cef_v8context_t*, void> on_context_created;

        [NativeTypeName("void (*)(struct _cef_render_process_handler_t *, struct _cef_browser_t *, struct _cef_frame_t *, struct _cef_v8context_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_render_process_handler_t*, _cef_browser_t*, _cef_frame_t*, _cef_v8context_t*, void> on_context_released;

        [NativeTypeName("void (*)(struct _cef_render_process_handler_t *, struct _cef_browser_t *, struct _cef_frame_t *, struct _cef_v8context_t *, struct _cef_v8exception_t *, struct _cef_v8stack_trace_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_render_process_handler_t*, _cef_browser_t*, _cef_frame_t*, _cef_v8context_t*, _cef_v8exception_t*, _cef_v8stack_trace_t*, void> on_uncaught_exception;

        [NativeTypeName("void (*)(struct _cef_render_process_handler_t *, struct _cef_browser_t *, struct _cef_frame_t *, struct _cef_domnode_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_render_process_handler_t*, _cef_browser_t*, _cef_frame_t*, _cef_domnode_t*, void> on_focused_node_changed;

        [NativeTypeName("int (*)(struct _cef_render_process_handler_t *, struct _cef_browser_t *, struct _cef_frame_t *, cef_process_id_t, struct _cef_process_message_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_render_process_handler_t*, _cef_browser_t*, _cef_frame_t*, cef_process_id_t, _cef_process_message_t*, int> on_process_message_received;
    }
}
