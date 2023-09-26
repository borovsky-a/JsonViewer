
namespace JsonViewer.Model
{
    public class JsonReaderResponse
    {
        public string Error { get; set; }
        public JsonItem Value { get; set; }
        public int MaxIndex { get; set; }
    }
}
