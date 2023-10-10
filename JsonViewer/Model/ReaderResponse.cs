
using System.Collections.Generic;

namespace JsonViewer.Model
{
    public sealed class ReaderResponse
    {
        public ReaderResponse()
        {
            ItemsList = new List<JsonItem>();
        }
        public string? Error { get; set; }
        public JsonItem? Value { get; set; }
        public List<JsonItem> ItemsList { get; set; }  
        public int MaxIndex { get; set; }

        public ReaderResponse WithError(string error)
        {
            Error = error;
            return this;
        }
    }
}
