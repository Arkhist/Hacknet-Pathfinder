namespace CefInterop
{
    public unsafe partial struct _cef_image_t
    {
        [NativeTypeName("cef_base_ref_counted_t")]
        public _cef_base_ref_counted_t @base;

        [NativeTypeName("int (*)(struct _cef_image_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_image_t*, int> is_empty;

        [NativeTypeName("int (*)(struct _cef_image_t *, struct _cef_image_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_image_t*, _cef_image_t*, int> is_same;

        [NativeTypeName("int (*)(struct _cef_image_t *, float, int, int, cef_color_type_t, cef_alpha_type_t, const void *, size_t) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_image_t*, float, int, int, cef_color_type_t, cef_alpha_type_t, void*, nuint, int> add_bitmap;

        [NativeTypeName("int (*)(struct _cef_image_t *, float, const void *, size_t) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_image_t*, float, void*, nuint, int> add_png;

        [NativeTypeName("int (*)(struct _cef_image_t *, float, const void *, size_t) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_image_t*, float, void*, nuint, int> add_jpeg;

        [NativeTypeName("size_t (*)(struct _cef_image_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_image_t*, nuint> get_width;

        [NativeTypeName("size_t (*)(struct _cef_image_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_image_t*, nuint> get_height;

        [NativeTypeName("int (*)(struct _cef_image_t *, float) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_image_t*, float, int> has_representation;

        [NativeTypeName("int (*)(struct _cef_image_t *, float) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_image_t*, float, int> remove_representation;

        [NativeTypeName("int (*)(struct _cef_image_t *, float, float *, int *, int *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_image_t*, float, float*, int*, int*, int> get_representation_info;

        [NativeTypeName("struct _cef_binary_value_t *(*)(struct _cef_image_t *, float, cef_color_type_t, cef_alpha_type_t, int *, int *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_image_t*, float, cef_color_type_t, cef_alpha_type_t, int*, int*, _cef_binary_value_t*> get_as_bitmap;

        [NativeTypeName("struct _cef_binary_value_t *(*)(struct _cef_image_t *, float, int, int *, int *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_image_t*, float, int, int*, int*, _cef_binary_value_t*> get_as_png;

        [NativeTypeName("struct _cef_binary_value_t *(*)(struct _cef_image_t *, float, int, int *, int *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_image_t*, float, int, int*, int*, _cef_binary_value_t*> get_as_jpeg;
    }
}
