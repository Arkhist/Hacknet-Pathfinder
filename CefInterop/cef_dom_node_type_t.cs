namespace CefInterop
{
    public enum cef_dom_node_type_t
    {
        DOM_NODE_TYPE_UNSUPPORTED = 0,
        DOM_NODE_TYPE_ELEMENT,
        DOM_NODE_TYPE_ATTRIBUTE,
        DOM_NODE_TYPE_TEXT,
        DOM_NODE_TYPE_CDATA_SECTION,
        DOM_NODE_TYPE_PROCESSING_INSTRUCTIONS,
        DOM_NODE_TYPE_COMMENT,
        DOM_NODE_TYPE_DOCUMENT,
        DOM_NODE_TYPE_DOCUMENT_TYPE,
        DOM_NODE_TYPE_DOCUMENT_FRAGMENT,
    }
}
