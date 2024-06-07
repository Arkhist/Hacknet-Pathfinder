namespace CefInterop
{
    public unsafe partial struct _cef_download_item_t
    {
        [NativeTypeName("cef_base_ref_counted_t")]
        public _cef_base_ref_counted_t @base;

        [NativeTypeName("int (*)(struct _cef_download_item_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_download_item_t*, int> is_valid;

        [NativeTypeName("int (*)(struct _cef_download_item_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_download_item_t*, int> is_in_progress;

        [NativeTypeName("int (*)(struct _cef_download_item_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_download_item_t*, int> is_complete;

        [NativeTypeName("int (*)(struct _cef_download_item_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_download_item_t*, int> is_canceled;

        [NativeTypeName("int (*)(struct _cef_download_item_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_download_item_t*, int> is_interrupted;

        [NativeTypeName("cef_download_interrupt_reason_t (*)(struct _cef_download_item_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_download_item_t*, cef_download_interrupt_reason_t> get_interrupt_reason;

        [NativeTypeName("int64_t (*)(struct _cef_download_item_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_download_item_t*, long> get_current_speed;

        [NativeTypeName("int (*)(struct _cef_download_item_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_download_item_t*, int> get_percent_complete;

        [NativeTypeName("int64_t (*)(struct _cef_download_item_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_download_item_t*, long> get_total_bytes;

        [NativeTypeName("int64_t (*)(struct _cef_download_item_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_download_item_t*, long> get_received_bytes;

        [NativeTypeName("cef_basetime_t (*)(struct _cef_download_item_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_download_item_t*, _cef_basetime_t> get_start_time;

        [NativeTypeName("cef_basetime_t (*)(struct _cef_download_item_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_download_item_t*, _cef_basetime_t> get_end_time;

        [NativeTypeName("cef_string_userfree_t (*)(struct _cef_download_item_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_download_item_t*, _cef_string_utf16_t*> get_full_path;

        [NativeTypeName("uint32_t (*)(struct _cef_download_item_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_download_item_t*, uint> get_id;

        [NativeTypeName("cef_string_userfree_t (*)(struct _cef_download_item_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_download_item_t*, _cef_string_utf16_t*> get_url;

        [NativeTypeName("cef_string_userfree_t (*)(struct _cef_download_item_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_download_item_t*, _cef_string_utf16_t*> get_original_url;

        [NativeTypeName("cef_string_userfree_t (*)(struct _cef_download_item_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_download_item_t*, _cef_string_utf16_t*> get_suggested_file_name;

        [NativeTypeName("cef_string_userfree_t (*)(struct _cef_download_item_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_download_item_t*, _cef_string_utf16_t*> get_content_disposition;

        [NativeTypeName("cef_string_userfree_t (*)(struct _cef_download_item_t *) __attribute__((stdcall))")]
        public delegate* unmanaged<_cef_download_item_t*, _cef_string_utf16_t*> get_mime_type;
    }
}
