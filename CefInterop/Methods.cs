using System.Runtime.InteropServices;

namespace CefInterop
{
    public static unsafe partial class Methods
    {
        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern int cef_get_min_log_level();

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern int cef_get_vlog_level([NativeTypeName("const char *")] sbyte* file_start, [NativeTypeName("size_t")] nuint N);

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern void cef_log([NativeTypeName("const char *")] sbyte* file, int line, int severity, [NativeTypeName("const char *")] sbyte* message);

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern int cef_string_wide_set([NativeTypeName("const wchar_t *")] ushort* src, [NativeTypeName("size_t")] nuint src_len, [NativeTypeName("cef_string_wide_t *")] _cef_string_wide_t* output, int copy);

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern int cef_string_utf8_set([NativeTypeName("const char *")] sbyte* src, [NativeTypeName("size_t")] nuint src_len, [NativeTypeName("cef_string_utf8_t *")] _cef_string_utf8_t* output, int copy);

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern int cef_string_utf16_set([NativeTypeName("const char16_t *")] ushort* src, [NativeTypeName("size_t")] nuint src_len, [NativeTypeName("cef_string_utf16_t *")] _cef_string_utf16_t* output, int copy);

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern void cef_string_wide_clear([NativeTypeName("cef_string_wide_t *")] _cef_string_wide_t* str);

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern void cef_string_utf8_clear([NativeTypeName("cef_string_utf8_t *")] _cef_string_utf8_t* str);

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern void cef_string_utf16_clear([NativeTypeName("cef_string_utf16_t *")] _cef_string_utf16_t* str);

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern int cef_string_wide_cmp([NativeTypeName("const cef_string_wide_t *")] _cef_string_wide_t* str1, [NativeTypeName("const cef_string_wide_t *")] _cef_string_wide_t* str2);

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern int cef_string_utf8_cmp([NativeTypeName("const cef_string_utf8_t *")] _cef_string_utf8_t* str1, [NativeTypeName("const cef_string_utf8_t *")] _cef_string_utf8_t* str2);

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern int cef_string_utf16_cmp([NativeTypeName("const cef_string_utf16_t *")] _cef_string_utf16_t* str1, [NativeTypeName("const cef_string_utf16_t *")] _cef_string_utf16_t* str2);

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern int cef_string_wide_to_utf8([NativeTypeName("const wchar_t *")] ushort* src, [NativeTypeName("size_t")] nuint src_len, [NativeTypeName("cef_string_utf8_t *")] _cef_string_utf8_t* output);

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern int cef_string_utf8_to_wide([NativeTypeName("const char *")] sbyte* src, [NativeTypeName("size_t")] nuint src_len, [NativeTypeName("cef_string_wide_t *")] _cef_string_wide_t* output);

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern int cef_string_wide_to_utf16([NativeTypeName("const wchar_t *")] ushort* src, [NativeTypeName("size_t")] nuint src_len, [NativeTypeName("cef_string_utf16_t *")] _cef_string_utf16_t* output);

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern int cef_string_utf16_to_wide([NativeTypeName("const char16_t *")] ushort* src, [NativeTypeName("size_t")] nuint src_len, [NativeTypeName("cef_string_wide_t *")] _cef_string_wide_t* output);

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern int cef_string_utf8_to_utf16([NativeTypeName("const char *")] sbyte* src, [NativeTypeName("size_t")] nuint src_len, [NativeTypeName("cef_string_utf16_t *")] _cef_string_utf16_t* output);

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern int cef_string_utf16_to_utf8([NativeTypeName("const char16_t *")] ushort* src, [NativeTypeName("size_t")] nuint src_len, [NativeTypeName("cef_string_utf8_t *")] _cef_string_utf8_t* output);

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern int cef_string_ascii_to_wide([NativeTypeName("const char *")] sbyte* src, [NativeTypeName("size_t")] nuint src_len, [NativeTypeName("cef_string_wide_t *")] _cef_string_wide_t* output);

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern int cef_string_ascii_to_utf16([NativeTypeName("const char *")] sbyte* src, [NativeTypeName("size_t")] nuint src_len, [NativeTypeName("cef_string_utf16_t *")] _cef_string_utf16_t* output);

        [DllImport("libcef.dll", ExactSpelling = true)]
        [return: NativeTypeName("cef_string_userfree_wide_t")]
        public static extern _cef_string_wide_t* cef_string_userfree_wide_alloc();

        [DllImport("libcef.dll", ExactSpelling = true)]
        [return: NativeTypeName("cef_string_userfree_utf8_t")]
        public static extern _cef_string_utf8_t* cef_string_userfree_utf8_alloc();

        [DllImport("libcef.dll", ExactSpelling = true)]
        [return: NativeTypeName("cef_string_userfree_utf16_t")]
        public static extern _cef_string_utf16_t* cef_string_userfree_utf16_alloc();

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern void cef_string_userfree_wide_free([NativeTypeName("cef_string_userfree_wide_t")] _cef_string_wide_t* str);

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern void cef_string_userfree_utf8_free([NativeTypeName("cef_string_userfree_utf8_t")] _cef_string_utf8_t* str);

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern void cef_string_userfree_utf16_free([NativeTypeName("cef_string_userfree_utf16_t")] _cef_string_utf16_t* str);

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern int cef_string_utf16_to_lower([NativeTypeName("const char16_t *")] ushort* src, [NativeTypeName("size_t")] nuint src_len, [NativeTypeName("cef_string_utf16_t *")] _cef_string_utf16_t* output);

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern int cef_string_utf16_to_upper([NativeTypeName("const char16_t *")] ushort* src, [NativeTypeName("size_t")] nuint src_len, [NativeTypeName("cef_string_utf16_t *")] _cef_string_utf16_t* output);

        [DllImport("libcef.dll", ExactSpelling = true)]
        [return: NativeTypeName("cef_string_list_t")]
        public static extern _cef_string_list_t* cef_string_list_alloc();

        [DllImport("libcef.dll", ExactSpelling = true)]
        [return: NativeTypeName("size_t")]
        public static extern nuint cef_string_list_size([NativeTypeName("cef_string_list_t")] _cef_string_list_t* list);

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern int cef_string_list_value([NativeTypeName("cef_string_list_t")] _cef_string_list_t* list, [NativeTypeName("size_t")] nuint index, [NativeTypeName("cef_string_t *")] _cef_string_utf16_t* value);

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern void cef_string_list_append([NativeTypeName("cef_string_list_t")] _cef_string_list_t* list, [NativeTypeName("const cef_string_t *")] _cef_string_utf16_t* value);

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern void cef_string_list_clear([NativeTypeName("cef_string_list_t")] _cef_string_list_t* list);

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern void cef_string_list_free([NativeTypeName("cef_string_list_t")] _cef_string_list_t* list);

        [DllImport("libcef.dll", ExactSpelling = true)]
        [return: NativeTypeName("cef_string_list_t")]
        public static extern _cef_string_list_t* cef_string_list_copy([NativeTypeName("cef_string_list_t")] _cef_string_list_t* list);

        [DllImport("libcef.dll", ExactSpelling = true)]
        [return: NativeTypeName("cef_string_map_t")]
        public static extern _cef_string_map_t* cef_string_map_alloc();

        [DllImport("libcef.dll", ExactSpelling = true)]
        [return: NativeTypeName("size_t")]
        public static extern nuint cef_string_map_size([NativeTypeName("cef_string_map_t")] _cef_string_map_t* map);

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern int cef_string_map_find([NativeTypeName("cef_string_map_t")] _cef_string_map_t* map, [NativeTypeName("const cef_string_t *")] _cef_string_utf16_t* key, [NativeTypeName("cef_string_t *")] _cef_string_utf16_t* value);

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern int cef_string_map_key([NativeTypeName("cef_string_map_t")] _cef_string_map_t* map, [NativeTypeName("size_t")] nuint index, [NativeTypeName("cef_string_t *")] _cef_string_utf16_t* key);

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern int cef_string_map_value([NativeTypeName("cef_string_map_t")] _cef_string_map_t* map, [NativeTypeName("size_t")] nuint index, [NativeTypeName("cef_string_t *")] _cef_string_utf16_t* value);

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern int cef_string_map_append([NativeTypeName("cef_string_map_t")] _cef_string_map_t* map, [NativeTypeName("const cef_string_t *")] _cef_string_utf16_t* key, [NativeTypeName("const cef_string_t *")] _cef_string_utf16_t* value);

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern void cef_string_map_clear([NativeTypeName("cef_string_map_t")] _cef_string_map_t* map);

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern void cef_string_map_free([NativeTypeName("cef_string_map_t")] _cef_string_map_t* map);

        [DllImport("libcef.dll", ExactSpelling = true)]
        [return: NativeTypeName("cef_string_multimap_t")]
        public static extern _cef_string_multimap_t* cef_string_multimap_alloc();

        [DllImport("libcef.dll", ExactSpelling = true)]
        [return: NativeTypeName("size_t")]
        public static extern nuint cef_string_multimap_size([NativeTypeName("cef_string_multimap_t")] _cef_string_multimap_t* map);

        [DllImport("libcef.dll", ExactSpelling = true)]
        [return: NativeTypeName("size_t")]
        public static extern nuint cef_string_multimap_find_count([NativeTypeName("cef_string_multimap_t")] _cef_string_multimap_t* map, [NativeTypeName("const cef_string_t *")] _cef_string_utf16_t* key);

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern int cef_string_multimap_enumerate([NativeTypeName("cef_string_multimap_t")] _cef_string_multimap_t* map, [NativeTypeName("const cef_string_t *")] _cef_string_utf16_t* key, [NativeTypeName("size_t")] nuint value_index, [NativeTypeName("cef_string_t *")] _cef_string_utf16_t* value);

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern int cef_string_multimap_key([NativeTypeName("cef_string_multimap_t")] _cef_string_multimap_t* map, [NativeTypeName("size_t")] nuint index, [NativeTypeName("cef_string_t *")] _cef_string_utf16_t* key);

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern int cef_string_multimap_value([NativeTypeName("cef_string_multimap_t")] _cef_string_multimap_t* map, [NativeTypeName("size_t")] nuint index, [NativeTypeName("cef_string_t *")] _cef_string_utf16_t* value);

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern int cef_string_multimap_append([NativeTypeName("cef_string_multimap_t")] _cef_string_multimap_t* map, [NativeTypeName("const cef_string_t *")] _cef_string_utf16_t* key, [NativeTypeName("const cef_string_t *")] _cef_string_utf16_t* value);

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern void cef_string_multimap_clear([NativeTypeName("cef_string_multimap_t")] _cef_string_multimap_t* map);

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern void cef_string_multimap_free([NativeTypeName("cef_string_multimap_t")] _cef_string_multimap_t* map);

        [DllImport("libcef.dll", ExactSpelling = true)]
        [return: NativeTypeName("cef_platform_thread_id_t")]
        public static extern uint cef_get_current_platform_thread_id();

        [DllImport("libcef.dll", ExactSpelling = true)]
        [return: NativeTypeName("cef_platform_thread_handle_t")]
        public static extern uint cef_get_current_platform_thread_handle();

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern int cef_time_to_timet([NativeTypeName("const cef_time_t *")] _cef_time_t* cef_time, [NativeTypeName("time_t *")] long* time);

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern int cef_time_from_timet([NativeTypeName("time_t")] long time, [NativeTypeName("cef_time_t *")] _cef_time_t* cef_time);

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern int cef_time_to_doublet([NativeTypeName("const cef_time_t *")] _cef_time_t* cef_time, double* time);

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern int cef_time_from_doublet(double time, [NativeTypeName("cef_time_t *")] _cef_time_t* cef_time);

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern int cef_time_now([NativeTypeName("cef_time_t *")] _cef_time_t* cef_time);

        [DllImport("libcef.dll", ExactSpelling = true)]
        [return: NativeTypeName("cef_basetime_t")]
        public static extern _cef_basetime_t cef_basetime_now();

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern int cef_time_delta([NativeTypeName("const cef_time_t *")] _cef_time_t* cef_time1, [NativeTypeName("const cef_time_t *")] _cef_time_t* cef_time2, [NativeTypeName("long long *")] long* delta);

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern int cef_time_to_basetime([NativeTypeName("const cef_time_t *")] _cef_time_t* from, [NativeTypeName("cef_basetime_t *")] _cef_basetime_t* to);

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern int cef_time_from_basetime([NativeTypeName("const cef_basetime_t")] _cef_basetime_t from, [NativeTypeName("cef_time_t *")] _cef_time_t* to);

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern void cef_trace_event_instant([NativeTypeName("const char *")] sbyte* category, [NativeTypeName("const char *")] sbyte* name, [NativeTypeName("const char *")] sbyte* arg1_name, [NativeTypeName("uint64_t")] ulong arg1_val, [NativeTypeName("const char *")] sbyte* arg2_name, [NativeTypeName("uint64_t")] ulong arg2_val);

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern void cef_trace_event_begin([NativeTypeName("const char *")] sbyte* category, [NativeTypeName("const char *")] sbyte* name, [NativeTypeName("const char *")] sbyte* arg1_name, [NativeTypeName("uint64_t")] ulong arg1_val, [NativeTypeName("const char *")] sbyte* arg2_name, [NativeTypeName("uint64_t")] ulong arg2_val);

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern void cef_trace_event_end([NativeTypeName("const char *")] sbyte* category, [NativeTypeName("const char *")] sbyte* name, [NativeTypeName("const char *")] sbyte* arg1_name, [NativeTypeName("uint64_t")] ulong arg1_val, [NativeTypeName("const char *")] sbyte* arg2_name, [NativeTypeName("uint64_t")] ulong arg2_val);

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern void cef_trace_counter([NativeTypeName("const char *")] sbyte* category, [NativeTypeName("const char *")] sbyte* name, [NativeTypeName("const char *")] sbyte* value1_name, [NativeTypeName("uint64_t")] ulong value1_val, [NativeTypeName("const char *")] sbyte* value2_name, [NativeTypeName("uint64_t")] ulong value2_val);

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern void cef_trace_counter_id([NativeTypeName("const char *")] sbyte* category, [NativeTypeName("const char *")] sbyte* name, [NativeTypeName("uint64_t")] ulong id, [NativeTypeName("const char *")] sbyte* value1_name, [NativeTypeName("uint64_t")] ulong value1_val, [NativeTypeName("const char *")] sbyte* value2_name, [NativeTypeName("uint64_t")] ulong value2_val);

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern void cef_trace_event_async_begin([NativeTypeName("const char *")] sbyte* category, [NativeTypeName("const char *")] sbyte* name, [NativeTypeName("uint64_t")] ulong id, [NativeTypeName("const char *")] sbyte* arg1_name, [NativeTypeName("uint64_t")] ulong arg1_val, [NativeTypeName("const char *")] sbyte* arg2_name, [NativeTypeName("uint64_t")] ulong arg2_val);

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern void cef_trace_event_async_step_into([NativeTypeName("const char *")] sbyte* category, [NativeTypeName("const char *")] sbyte* name, [NativeTypeName("uint64_t")] ulong id, [NativeTypeName("uint64_t")] ulong step, [NativeTypeName("const char *")] sbyte* arg1_name, [NativeTypeName("uint64_t")] ulong arg1_val);

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern void cef_trace_event_async_step_past([NativeTypeName("const char *")] sbyte* category, [NativeTypeName("const char *")] sbyte* name, [NativeTypeName("uint64_t")] ulong id, [NativeTypeName("uint64_t")] ulong step, [NativeTypeName("const char *")] sbyte* arg1_name, [NativeTypeName("uint64_t")] ulong arg1_val);

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern void cef_trace_event_async_end([NativeTypeName("const char *")] sbyte* category, [NativeTypeName("const char *")] sbyte* name, [NativeTypeName("uint64_t")] ulong id, [NativeTypeName("const char *")] sbyte* arg1_name, [NativeTypeName("uint64_t")] ulong arg1_val, [NativeTypeName("const char *")] sbyte* arg2_name, [NativeTypeName("uint64_t")] ulong arg2_val);

        [DllImport("libcef.dll", ExactSpelling = true)]
        [return: NativeTypeName("cef_value_t *")]
        public static extern _cef_value_t* cef_value_create();

        [DllImport("libcef.dll", ExactSpelling = true)]
        [return: NativeTypeName("cef_binary_value_t *")]
        public static extern _cef_binary_value_t* cef_binary_value_create([NativeTypeName("const void *")] void* data, [NativeTypeName("size_t")] nuint data_size);

        [DllImport("libcef.dll", ExactSpelling = true)]
        [return: NativeTypeName("cef_dictionary_value_t *")]
        public static extern _cef_dictionary_value_t* cef_dictionary_value_create();

        [DllImport("libcef.dll", ExactSpelling = true)]
        [return: NativeTypeName("cef_list_value_t *")]
        public static extern _cef_list_value_t* cef_list_value_create();

        [DllImport("libcef.dll", ExactSpelling = true)]
        [return: NativeTypeName("cef_image_t *")]
        public static extern _cef_image_t* cef_image_create();

        [DllImport("libcef.dll", ExactSpelling = true)]
        [return: NativeTypeName("cef_stream_reader_t *")]
        public static extern _cef_stream_reader_t* cef_stream_reader_create_for_file([NativeTypeName("const cef_string_t *")] _cef_string_utf16_t* fileName);

        [DllImport("libcef.dll", ExactSpelling = true)]
        [return: NativeTypeName("cef_stream_reader_t *")]
        public static extern _cef_stream_reader_t* cef_stream_reader_create_for_data(void* data, [NativeTypeName("size_t")] nuint size);

        [DllImport("libcef.dll", ExactSpelling = true)]
        [return: NativeTypeName("cef_stream_reader_t *")]
        public static extern _cef_stream_reader_t* cef_stream_reader_create_for_handler([NativeTypeName("cef_read_handler_t *")] _cef_read_handler_t* handler);

        [DllImport("libcef.dll", ExactSpelling = true)]
        [return: NativeTypeName("cef_stream_writer_t *")]
        public static extern _cef_stream_writer_t* cef_stream_writer_create_for_file([NativeTypeName("const cef_string_t *")] _cef_string_utf16_t* fileName);

        [DllImport("libcef.dll", ExactSpelling = true)]
        [return: NativeTypeName("cef_stream_writer_t *")]
        public static extern _cef_stream_writer_t* cef_stream_writer_create_for_handler([NativeTypeName("cef_write_handler_t *")] _cef_write_handler_t* handler);

        [DllImport("libcef.dll", ExactSpelling = true)]
        [return: NativeTypeName("cef_drag_data_t *")]
        public static extern _cef_drag_data_t* cef_drag_data_create();

        [DllImport("libcef.dll", ExactSpelling = true)]
        [return: NativeTypeName("cef_process_message_t *")]
        public static extern _cef_process_message_t* cef_process_message_create([NativeTypeName("const cef_string_t *")] _cef_string_utf16_t* name);

        [DllImport("libcef.dll", ExactSpelling = true)]
        [return: NativeTypeName("cef_request_t *")]
        public static extern _cef_request_t* cef_request_create();

        [DllImport("libcef.dll", ExactSpelling = true)]
        [return: NativeTypeName("cef_post_data_t *")]
        public static extern _cef_post_data_t* cef_post_data_create();

        [DllImport("libcef.dll", ExactSpelling = true)]
        [return: NativeTypeName("cef_post_data_element_t *")]
        public static extern _cef_post_data_element_t* cef_post_data_element_create();

        [DllImport("libcef.dll", ExactSpelling = true)]
        [return: NativeTypeName("cef_cookie_manager_t *")]
        public static extern _cef_cookie_manager_t* cef_cookie_manager_get_global_manager([NativeTypeName("struct _cef_completion_callback_t *")] _cef_completion_callback_t* callback);

        [DllImport("libcef.dll", ExactSpelling = true)]
        [return: NativeTypeName("cef_media_router_t *")]
        public static extern _cef_media_router_t* cef_media_router_get_global([NativeTypeName("struct _cef_completion_callback_t *")] _cef_completion_callback_t* callback);

        [DllImport("libcef.dll", ExactSpelling = true)]
        [return: NativeTypeName("cef_preference_manager_t *")]
        public static extern _cef_preference_manager_t* cef_preference_manager_get_global();

        [DllImport("libcef.dll", ExactSpelling = true)]
        [return: NativeTypeName("cef_request_context_t *")]
        public static extern _cef_request_context_t* cef_request_context_get_global_context();

        [DllImport("libcef.dll", ExactSpelling = true)]
        [return: NativeTypeName("cef_request_context_t *")]
        public static extern _cef_request_context_t* cef_request_context_create_context([NativeTypeName("const struct _cef_request_context_settings_t *")] _cef_request_context_settings_t* settings, [NativeTypeName("struct _cef_request_context_handler_t *")] _cef_request_context_handler_t* handler);

        [DllImport("libcef.dll", ExactSpelling = true)]
        [return: NativeTypeName("cef_request_context_t *")]
        public static extern _cef_request_context_t* cef_create_context_shared([NativeTypeName("cef_request_context_t *")] _cef_request_context_t* other, [NativeTypeName("struct _cef_request_context_handler_t *")] _cef_request_context_handler_t* handler);

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern int cef_browser_host_create_browser([NativeTypeName("const cef_window_info_t *")] _cef_window_info_t* windowInfo, [NativeTypeName("struct _cef_client_t *")] _cef_client_t* client, [NativeTypeName("const cef_string_t *")] _cef_string_utf16_t* url, [NativeTypeName("const struct _cef_browser_settings_t *")] _cef_browser_settings_t* settings, [NativeTypeName("struct _cef_dictionary_value_t *")] _cef_dictionary_value_t* extra_info, [NativeTypeName("struct _cef_request_context_t *")] _cef_request_context_t* request_context);

        [DllImport("libcef.dll", ExactSpelling = true)]
        [return: NativeTypeName("cef_browser_t *")]
        public static extern _cef_browser_t* cef_browser_host_create_browser_sync([NativeTypeName("const cef_window_info_t *")] _cef_window_info_t* windowInfo, [NativeTypeName("struct _cef_client_t *")] _cef_client_t* client, [NativeTypeName("const cef_string_t *")] _cef_string_utf16_t* url, [NativeTypeName("const struct _cef_browser_settings_t *")] _cef_browser_settings_t* settings, [NativeTypeName("struct _cef_dictionary_value_t *")] _cef_dictionary_value_t* extra_info, [NativeTypeName("struct _cef_request_context_t *")] _cef_request_context_t* request_context);

        [DllImport("libcef.dll", ExactSpelling = true)]
        [return: NativeTypeName("cef_menu_model_t *")]
        public static extern _cef_menu_model_t* cef_menu_model_create([NativeTypeName("struct _cef_menu_model_delegate_t *")] _cef_menu_model_delegate_t* @delegate);

        [DllImport("libcef.dll", ExactSpelling = true)]
        [return: NativeTypeName("cef_print_settings_t *")]
        public static extern _cef_print_settings_t* cef_print_settings_create();

        [DllImport("libcef.dll", ExactSpelling = true)]
        [return: NativeTypeName("cef_response_t *")]
        public static extern _cef_response_t* cef_response_create();

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern int cef_is_cert_status_error(cef_cert_status_t status);

        [DllImport("libcef.dll", ExactSpelling = true)]
        [return: NativeTypeName("cef_command_line_t *")]
        public static extern _cef_command_line_t* cef_command_line_create();

        [DllImport("libcef.dll", ExactSpelling = true)]
        [return: NativeTypeName("cef_command_line_t *")]
        public static extern _cef_command_line_t* cef_command_line_get_global();

        [DllImport("libcef.dll", ExactSpelling = true)]
        [return: NativeTypeName("cef_task_runner_t *")]
        public static extern _cef_task_runner_t* cef_task_runner_get_for_current_thread();

        [DllImport("libcef.dll", ExactSpelling = true)]
        [return: NativeTypeName("cef_task_runner_t *")]
        public static extern _cef_task_runner_t* cef_task_runner_get_for_thread(cef_thread_id_t threadId);

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern int cef_currently_on(cef_thread_id_t threadId);

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern int cef_post_task(cef_thread_id_t threadId, [NativeTypeName("cef_task_t *")] _cef_task_t* task);

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern int cef_post_delayed_task(cef_thread_id_t threadId, [NativeTypeName("cef_task_t *")] _cef_task_t* task, [NativeTypeName("int64_t")] long delay_ms);

        [DllImport("libcef.dll", ExactSpelling = true)]
        [return: NativeTypeName("cef_v8context_t *")]
        public static extern _cef_v8context_t* cef_v8context_get_current_context();

        [DllImport("libcef.dll", ExactSpelling = true)]
        [return: NativeTypeName("cef_v8context_t *")]
        public static extern _cef_v8context_t* cef_v8context_get_entered_context();

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern int cef_v8context_in_context();

        [DllImport("libcef.dll", ExactSpelling = true)]
        [return: NativeTypeName("cef_v8value_t *")]
        public static extern _cef_v8value_t* cef_v8value_create_undefined();

        [DllImport("libcef.dll", ExactSpelling = true)]
        [return: NativeTypeName("cef_v8value_t *")]
        public static extern _cef_v8value_t* cef_v8value_create_null();

        [DllImport("libcef.dll", ExactSpelling = true)]
        [return: NativeTypeName("cef_v8value_t *")]
        public static extern _cef_v8value_t* cef_v8value_create_bool(int value);

        [DllImport("libcef.dll", ExactSpelling = true)]
        [return: NativeTypeName("cef_v8value_t *")]
        public static extern _cef_v8value_t* cef_v8value_create_int([NativeTypeName("int32_t")] int value);

        [DllImport("libcef.dll", ExactSpelling = true)]
        [return: NativeTypeName("cef_v8value_t *")]
        public static extern _cef_v8value_t* cef_v8value_create_uint([NativeTypeName("uint32_t")] uint value);

        [DllImport("libcef.dll", ExactSpelling = true)]
        [return: NativeTypeName("cef_v8value_t *")]
        public static extern _cef_v8value_t* cef_v8value_create_double(double value);

        [DllImport("libcef.dll", ExactSpelling = true)]
        [return: NativeTypeName("cef_v8value_t *")]
        public static extern _cef_v8value_t* cef_v8value_create_date([NativeTypeName("cef_basetime_t")] _cef_basetime_t date);

        [DllImport("libcef.dll", ExactSpelling = true)]
        [return: NativeTypeName("cef_v8value_t *")]
        public static extern _cef_v8value_t* cef_v8value_create_string([NativeTypeName("const cef_string_t *")] _cef_string_utf16_t* value);

        [DllImport("libcef.dll", ExactSpelling = true)]
        [return: NativeTypeName("cef_v8value_t *")]
        public static extern _cef_v8value_t* cef_v8value_create_object([NativeTypeName("cef_v8accessor_t *")] _cef_v8accessor_t* accessor, [NativeTypeName("cef_v8interceptor_t *")] _cef_v8interceptor_t* interceptor);

        [DllImport("libcef.dll", ExactSpelling = true)]
        [return: NativeTypeName("cef_v8value_t *")]
        public static extern _cef_v8value_t* cef_v8value_create_array(int length);

        [DllImport("libcef.dll", ExactSpelling = true)]
        [return: NativeTypeName("cef_v8value_t *")]
        public static extern _cef_v8value_t* cef_v8value_create_array_buffer(void* buffer, [NativeTypeName("size_t")] nuint length, [NativeTypeName("cef_v8array_buffer_release_callback_t *")] _cef_v8array_buffer_release_callback_t* release_callback);

        [DllImport("libcef.dll", ExactSpelling = true)]
        [return: NativeTypeName("cef_v8value_t *")]
        public static extern _cef_v8value_t* cef_v8value_create_function([NativeTypeName("const cef_string_t *")] _cef_string_utf16_t* name, [NativeTypeName("cef_v8handler_t *")] _cef_v8handler_t* handler);

        [DllImport("libcef.dll", ExactSpelling = true)]
        [return: NativeTypeName("cef_v8value_t *")]
        public static extern _cef_v8value_t* cef_v8value_create_promise();

        [DllImport("libcef.dll", ExactSpelling = true)]
        [return: NativeTypeName("cef_v8stack_trace_t *")]
        public static extern _cef_v8stack_trace_t* cef_v8stack_trace_get_current(int frame_limit);

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern int cef_register_extension([NativeTypeName("const cef_string_t *")] _cef_string_utf16_t* extension_name, [NativeTypeName("const cef_string_t *")] _cef_string_utf16_t* javascript_code, [NativeTypeName("cef_v8handler_t *")] _cef_v8handler_t* handler);

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern int cef_register_scheme_handler_factory([NativeTypeName("const cef_string_t *")] _cef_string_utf16_t* scheme_name, [NativeTypeName("const cef_string_t *")] _cef_string_utf16_t* domain_name, [NativeTypeName("cef_scheme_handler_factory_t *")] _cef_scheme_handler_factory_t* factory);

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern int cef_clear_scheme_handler_factories();

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern int cef_execute_process([NativeTypeName("const cef_main_args_t *")] _cef_main_args_t* args, [NativeTypeName("cef_app_t *")] _cef_app_t* application, void* windows_sandbox_info);

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern int cef_initialize([NativeTypeName("const cef_main_args_t *")] _cef_main_args_t* args, [NativeTypeName("const struct _cef_settings_t *")] _cef_settings_t* settings, [NativeTypeName("cef_app_t *")] _cef_app_t* application, void* windows_sandbox_info);

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern int cef_get_exit_code();

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern void cef_shutdown();

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern void cef_do_message_loop_work();

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern void cef_run_message_loop();

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern void cef_quit_message_loop();

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern int cef_crash_reporting_enabled();

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern void cef_set_crash_key_value([NativeTypeName("const cef_string_t *")] _cef_string_utf16_t* key, [NativeTypeName("const cef_string_t *")] _cef_string_utf16_t* value);

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern int cef_create_directory([NativeTypeName("const cef_string_t *")] _cef_string_utf16_t* full_path);

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern int cef_get_temp_directory([NativeTypeName("cef_string_t *")] _cef_string_utf16_t* temp_dir);

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern int cef_create_new_temp_directory([NativeTypeName("const cef_string_t *")] _cef_string_utf16_t* prefix, [NativeTypeName("cef_string_t *")] _cef_string_utf16_t* new_temp_path);

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern int cef_create_temp_directory_in_directory([NativeTypeName("const cef_string_t *")] _cef_string_utf16_t* base_dir, [NativeTypeName("const cef_string_t *")] _cef_string_utf16_t* prefix, [NativeTypeName("cef_string_t *")] _cef_string_utf16_t* new_dir);

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern int cef_directory_exists([NativeTypeName("const cef_string_t *")] _cef_string_utf16_t* path);

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern int cef_delete_file([NativeTypeName("const cef_string_t *")] _cef_string_utf16_t* path, int recursive);

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern int cef_zip_directory([NativeTypeName("const cef_string_t *")] _cef_string_utf16_t* src_dir, [NativeTypeName("const cef_string_t *")] _cef_string_utf16_t* dest_file, int include_hidden_files);

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern void cef_load_crlsets_file([NativeTypeName("const cef_string_t *")] _cef_string_utf16_t* path);

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern int cef_is_rtl();

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern int cef_add_cross_origin_whitelist_entry([NativeTypeName("const cef_string_t *")] _cef_string_utf16_t* source_origin, [NativeTypeName("const cef_string_t *")] _cef_string_utf16_t* target_protocol, [NativeTypeName("const cef_string_t *")] _cef_string_utf16_t* target_domain, int allow_target_subdomains);

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern int cef_remove_cross_origin_whitelist_entry([NativeTypeName("const cef_string_t *")] _cef_string_utf16_t* source_origin, [NativeTypeName("const cef_string_t *")] _cef_string_utf16_t* target_protocol, [NativeTypeName("const cef_string_t *")] _cef_string_utf16_t* target_domain, int allow_target_subdomains);

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern int cef_clear_cross_origin_whitelist();

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern int cef_resolve_url([NativeTypeName("const cef_string_t *")] _cef_string_utf16_t* base_url, [NativeTypeName("const cef_string_t *")] _cef_string_utf16_t* relative_url, [NativeTypeName("cef_string_t *")] _cef_string_utf16_t* resolved_url);

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern int cef_parse_url([NativeTypeName("const cef_string_t *")] _cef_string_utf16_t* url, [NativeTypeName("struct _cef_urlparts_t *")] _cef_urlparts_t* parts);

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern int cef_create_url([NativeTypeName("const struct _cef_urlparts_t *")] _cef_urlparts_t* parts, [NativeTypeName("cef_string_t *")] _cef_string_utf16_t* url);

        [DllImport("libcef.dll", ExactSpelling = true)]
        [return: NativeTypeName("cef_string_userfree_t")]
        public static extern _cef_string_utf16_t* cef_format_url_for_security_display([NativeTypeName("const cef_string_t *")] _cef_string_utf16_t* origin_url);

        [DllImport("libcef.dll", ExactSpelling = true)]
        [return: NativeTypeName("cef_string_userfree_t")]
        public static extern _cef_string_utf16_t* cef_get_mime_type([NativeTypeName("const cef_string_t *")] _cef_string_utf16_t* extension);

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern void cef_get_extensions_for_mime_type([NativeTypeName("const cef_string_t *")] _cef_string_utf16_t* mime_type, [NativeTypeName("cef_string_list_t")] _cef_string_list_t* extensions);

        [DllImport("libcef.dll", ExactSpelling = true)]
        [return: NativeTypeName("cef_string_userfree_t")]
        public static extern _cef_string_utf16_t* cef_base64encode([NativeTypeName("const void *")] void* data, [NativeTypeName("size_t")] nuint data_size);

        [DllImport("libcef.dll", ExactSpelling = true)]
        [return: NativeTypeName("struct _cef_binary_value_t *")]
        public static extern _cef_binary_value_t* cef_base64decode([NativeTypeName("const cef_string_t *")] _cef_string_utf16_t* data);

        [DllImport("libcef.dll", ExactSpelling = true)]
        [return: NativeTypeName("cef_string_userfree_t")]
        public static extern _cef_string_utf16_t* cef_uriencode([NativeTypeName("const cef_string_t *")] _cef_string_utf16_t* text, int use_plus);

        [DllImport("libcef.dll", ExactSpelling = true)]
        [return: NativeTypeName("cef_string_userfree_t")]
        public static extern _cef_string_utf16_t* cef_uridecode([NativeTypeName("const cef_string_t *")] _cef_string_utf16_t* text, int convert_to_utf8, cef_uri_unescape_rule_t unescape_rule);

        [DllImport("libcef.dll", ExactSpelling = true)]
        [return: NativeTypeName("struct _cef_value_t *")]
        public static extern _cef_value_t* cef_parse_json([NativeTypeName("const cef_string_t *")] _cef_string_utf16_t* json_string, cef_json_parser_options_t options);

        [DllImport("libcef.dll", ExactSpelling = true)]
        [return: NativeTypeName("struct _cef_value_t *")]
        public static extern _cef_value_t* cef_parse_json_buffer([NativeTypeName("const void *")] void* json, [NativeTypeName("size_t")] nuint json_size, cef_json_parser_options_t options);

        [DllImport("libcef.dll", ExactSpelling = true)]
        [return: NativeTypeName("struct _cef_value_t *")]
        public static extern _cef_value_t* cef_parse_jsonand_return_error([NativeTypeName("const cef_string_t *")] _cef_string_utf16_t* json_string, cef_json_parser_options_t options, [NativeTypeName("cef_string_t *")] _cef_string_utf16_t* error_msg_out);

        [DllImport("libcef.dll", ExactSpelling = true)]
        [return: NativeTypeName("cef_string_userfree_t")]
        public static extern _cef_string_utf16_t* cef_write_json([NativeTypeName("struct _cef_value_t *")] _cef_value_t* node, cef_json_writer_options_t options);

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern int cef_get_path(cef_path_key_t key, [NativeTypeName("cef_string_t *")] _cef_string_utf16_t* path);

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern int cef_launch_process([NativeTypeName("struct _cef_command_line_t *")] _cef_command_line_t* command_line);

        [DllImport("libcef.dll", ExactSpelling = true)]
        [return: NativeTypeName("cef_resource_bundle_t *")]
        public static extern _cef_resource_bundle_t* cef_resource_bundle_get_global();

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern void cef_server_create([NativeTypeName("const cef_string_t *")] _cef_string_utf16_t* address, [NativeTypeName("uint16_t")] ushort port, int backlog, [NativeTypeName("struct _cef_server_handler_t *")] _cef_server_handler_t* handler);

        [DllImport("libcef.dll", ExactSpelling = true)]
        [return: NativeTypeName("cef_shared_process_message_builder_t *")]
        public static extern _cef_shared_process_message_builder_t* cef_shared_process_message_builder_create([NativeTypeName("const cef_string_t *")] _cef_string_utf16_t* name, [NativeTypeName("size_t")] nuint byte_size);

        [DllImport("libcef.dll", ExactSpelling = true)]
        [return: NativeTypeName("cef_thread_t *")]
        public static extern _cef_thread_t* cef_thread_create([NativeTypeName("const cef_string_t *")] _cef_string_utf16_t* display_name, cef_thread_priority_t priority, cef_message_loop_type_t message_loop_type, int stoppable, cef_com_init_mode_t com_init_mode);

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern int cef_begin_tracing([NativeTypeName("const cef_string_t *")] _cef_string_utf16_t* categories, [NativeTypeName("struct _cef_completion_callback_t *")] _cef_completion_callback_t* callback);

        [DllImport("libcef.dll", ExactSpelling = true)]
        public static extern int cef_end_tracing([NativeTypeName("const cef_string_t *")] _cef_string_utf16_t* tracing_file, [NativeTypeName("cef_end_tracing_callback_t *")] _cef_end_tracing_callback_t* callback);

        [DllImport("libcef.dll", ExactSpelling = true)]
        [return: NativeTypeName("int64_t")]
        public static extern long cef_now_from_system_trace_time();

        [DllImport("libcef.dll", ExactSpelling = true)]
        [return: NativeTypeName("cef_urlrequest_t *")]
        public static extern _cef_urlrequest_t* cef_urlrequest_create([NativeTypeName("struct _cef_request_t *")] _cef_request_t* request, [NativeTypeName("struct _cef_urlrequest_client_t *")] _cef_urlrequest_client_t* client, [NativeTypeName("struct _cef_request_context_t *")] _cef_request_context_t* request_context);

        [DllImport("libcef.dll", ExactSpelling = true)]
        [return: NativeTypeName("cef_waitable_event_t *")]
        public static extern _cef_waitable_event_t* cef_waitable_event_create(int automatic_reset, int initially_signaled);

        [DllImport("libcef.dll", ExactSpelling = true)]
        [return: NativeTypeName("cef_xml_reader_t *")]
        public static extern _cef_xml_reader_t* cef_xml_reader_create([NativeTypeName("struct _cef_stream_reader_t *")] _cef_stream_reader_t* stream, cef_xml_encoding_type_t encodingType, [NativeTypeName("const cef_string_t *")] _cef_string_utf16_t* URI);

        [DllImport("libcef.dll", ExactSpelling = true)]
        [return: NativeTypeName("cef_zip_reader_t *")]
        public static extern _cef_zip_reader_t* cef_zip_reader_create([NativeTypeName("struct _cef_stream_reader_t *")] _cef_stream_reader_t* stream);
    }
}
