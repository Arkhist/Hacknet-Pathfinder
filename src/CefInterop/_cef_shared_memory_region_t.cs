namespace CefInterop
{
    public unsafe partial struct _cef_shared_memory_region_t
    {
        [NativeTypeName("cef_base_ref_counted_t")]
        public _cef_base_ref_counted_t @base;

        [NativeTypeName("int (*)(struct _cef_shared_memory_region_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_shared_memory_region_t*, int> is_valid;

        [NativeTypeName("size_t (*)(struct _cef_shared_memory_region_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_shared_memory_region_t*, nuint> size;

        [NativeTypeName("void *(*)(struct _cef_shared_memory_region_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_shared_memory_region_t*, void*> memory;
    }
}
