
namespace JsonViewer.Model
{
    public sealed class ReaderResponse
    {
        public string Error { get; set; }
        public JsonItem Value { get; set; }
        public int MaxIndex { get; set; }

        public ReaderResponse WithError(string error)
        {
            Error = error;
            return this;
        }
    }
}
