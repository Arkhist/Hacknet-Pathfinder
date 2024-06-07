namespace CefInterop
{
    public unsafe partial struct _cef_waitable_event_t
    {
        [NativeTypeName("cef_base_ref_counted_t")]
        public _cef_base_ref_counted_t @base;

        [NativeTypeName("void (*)(struct _cef_waitable_event_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_waitable_event_t*, void> reset;

        [NativeTypeName("void (*)(struct _cef_waitable_event_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_waitable_event_t*, void> signal;

        [NativeTypeName("int (*)(struct _cef_waitable_event_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_waitable_event_t*, int> is_signaled;

        [NativeTypeName("void (*)(struct _cef_waitable_event_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_waitable_event_t*, void> wait;

        [NativeTypeName("int (*)(struct _cef_waitable_event_t *, int64_t) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_waitable_event_t*, long, int> timed_wait;
    }
}
