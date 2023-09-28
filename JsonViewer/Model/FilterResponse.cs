using System.Collections.Generic;

namespace JsonViewer.Model
{
    public sealed class FilterResponse
    {
        public FilterResponse()
        {
            Matches = new List<JsonItem>();
        }
        public List<JsonItem> Matches { get; set; }
        public JsonItem Selected { get; set; }
        public JsonItem Result { get; set; }

    }
}
