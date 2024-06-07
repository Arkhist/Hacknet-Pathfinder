namespace CefInterop
{
    public unsafe partial struct _cef_task_runner_t
    {
        [NativeTypeName("cef_base_ref_counted_t")]
        public _cef_base_ref_counted_t @base;

        [NativeTypeName("int (*)(struct _cef_task_runner_t *, struct _cef_task_runner_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_task_runner_t*, _cef_task_runner_t*, int> is_same;

        [NativeTypeName("int (*)(struct _cef_task_runner_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_task_runner_t*, int> belongs_to_current_thread;

        [NativeTypeName("int (*)(struct _cef_task_runner_t *, cef_thread_id_t) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_task_runner_t*, cef_thread_id_t, int> belongs_to_thread;

        [NativeTypeName("int (*)(struct _cef_task_runner_t *, struct _cef_task_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_task_runner_t*, _cef_task_t*, int> post_task;

        [NativeTypeName("int (*)(struct _cef_task_runner_t *, struct _cef_task_t *, int64_t) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_task_runner_t*, _cef_task_t*, long, int> post_delayed_task;
    }
}
