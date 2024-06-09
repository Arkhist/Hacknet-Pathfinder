namespace CefInterop
{
    public partial struct _cef_audio_parameters_t
    {
        public cef_channel_layout_t channel_layout;

        public int sample_rate;

        public int frames_per_buffer;
    }
}
