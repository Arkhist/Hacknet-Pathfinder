namespace CefInterop
{
    public unsafe partial struct _cef_preference_manager_t
    {
        [NativeTypeName("cef_base_ref_counted_t")]
        public _cef_base_ref_counted_t @base;

        [NativeTypeName("int (*)(struct _cef_preference_manager_t *, const cef_string_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_preference_manager_t*, _cef_string_utf16_t*, int> has_preference;

        [NativeTypeName("struct _cef_value_t *(*)(struct _cef_preference_manager_t *, const cef_string_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_preference_manager_t*, _cef_string_utf16_t*, _cef_value_t*> get_preference;

        [NativeTypeName("struct _cef_dictionary_value_t *(*)(struct _cef_preference_manager_t *, int) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_preference_manager_t*, int, _cef_dictionary_value_t*> get_all_preferences;

        [NativeTypeName("int (*)(struct _cef_preference_manager_t *, const cef_string_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_preference_manager_t*, _cef_string_utf16_t*, int> can_set_preference;

        [NativeTypeName("int (*)(struct _cef_preference_manager_t *, const cef_string_t *, struct _cef_value_t *, cef_string_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_preference_manager_t*, _cef_string_utf16_t*, _cef_value_t*, _cef_string_utf16_t*, int> set_preference;
    }
}
