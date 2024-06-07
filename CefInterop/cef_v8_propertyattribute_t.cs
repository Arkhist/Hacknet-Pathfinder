namespace CefInterop
{
    public enum cef_v8_propertyattribute_t
    {
        V8_PROPERTY_ATTRIBUTE_NONE = 0,
        V8_PROPERTY_ATTRIBUTE_READONLY = 1 << 0,
        V8_PROPERTY_ATTRIBUTE_DONTENUM = 1 << 1,
        V8_PROPERTY_ATTRIBUTE_DONTDELETE = 1 << 2,
    }
}
