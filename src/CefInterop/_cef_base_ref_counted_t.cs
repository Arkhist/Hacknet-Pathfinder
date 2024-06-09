namespace CefInterop
{
    public unsafe partial struct _cef_base_ref_counted_t
    {
        [NativeTypeName("size_t")]
        public nuint size;

        [NativeTypeName("void (*)(struct _cef_base_ref_counted_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_base_ref_counted_t*, void> add_ref;

        [NativeTypeName("int (*)(struct _cef_base_ref_counted_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_base_ref_counted_t*, int> release;

        [NativeTypeName("int (*)(struct _cef_base_ref_counted_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_base_ref_counted_t*, int> has_one_ref;

        [NativeTypeName("int (*)(struct _cef_base_ref_counted_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_base_ref_counted_t*, int> has_at_least_one_ref;
    }
}
