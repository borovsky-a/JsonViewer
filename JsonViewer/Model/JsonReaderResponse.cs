
namespace JsonViewer.Model
{
    public sealed class JsonReaderResponse
    {
        public string Error { get; set; }
        public JsonItem Value { get; set; }
        public int MaxIndex { get; set; }

        public JsonReaderResponse WithError(string error)
        {
            Error = error;
            return this;
        }
    }
}
