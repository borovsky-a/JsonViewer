using JsonViewer.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JsonViewer.Service
{
    public sealed class JsonViewerManager
    {
        private CancellationTokenSource _cts;
        private int _count;
        private List<JsonItem> _items;
        public JsonViewerManager()
        {
            _items = new List<JsonItem>();
        }

        public  IEnumerable<JsonItem> GetMatchItems()
        {
            return _items.Where(o => o.IsMatch);
        }

        public void CollapseItems()
        {
            for (int i = _items.Count - 1; i >= 0; i--)
            {
                _items[i].IsExpanded = false;
            }
        }

        public void ExpandItems()
        {
            for (int i = 0; i < _items.Count; i++)
            {
                _items[i].IsExpanded = true;
            }
        }

        public int FilteredItems(string filter)
        {
            if (string.IsNullOrEmpty(filter))
            {
                foreach (var item in _items)
                {
                    item.IsVisible = true;
                    item.IsMatch = false;
                }
                return _items.Count;
            }
            var matchesCount = 0;
            foreach (var item in _items)
            {
                if (string.IsNullOrEmpty(item.Name))
                {
                    continue;
                }
                if (ItemIsContainsTo(item, filter))
                {
                    matchesCount++;
                    item.IsMatch = true;
                    item.IsVisible = true;
                    item.SetParentsState(o =>
                    {
                        o.IsMatch = true;
                        o.IsExpanded = true;
                        o.IsVisible = true;
                    });
                }
                else
                {
                    item.IsVisible = false;
                }
            }
            return matchesCount;
        }

        private bool ItemIsContainsTo(JsonItem item, string value)
        {
            if(item.ItemType == JsonItemType.Value)
            {
                var itemValue = item.GetDisplayValue();
                return itemValue.ContainsIgnoreCase(value);
            }
            if (item.Name.ContainsIgnoreCase(value))
            {
                return true;
            }
            if(!string.IsNullOrEmpty(item.Value) && item.Value.ContainsIgnoreCase(value))
            {
                return true;
            }
            return false;
        }

        public void CancelReadFileCommand() => _cts?.Cancel();

        public async Task<ReaderResponse> ReadJson(string path)
        {
            if(_cts != null && !_cts.IsCancellationRequested)
            {
                _cts.Cancel();
            }
            _cts = new CancellationTokenSource();
            _count = 0;
            _items = new List<JsonItem>();
            var response = new ReaderResponse();
            if (string.IsNullOrEmpty(path))
            {
                return response.WithError("Не указан путь к файлу.");
            }
            try
            {
                if (!File.Exists(path))
                {
                    return response.WithError("Не указан путь к файлу.");
                }

                var jToken = default(JToken);
                using (var tReader = File.OpenText(path))
                using (var jReader = new JsonTextReader(tReader))
                {
                    jToken = await JToken.LoadAsync(jReader, _cts.Token);                     
                }
                if (jToken is JArray jArray)
                {
                    response.Value = ProcessArray(jArray, "root", null);
                }
                else if (jToken is JObject jObject)
                {
                    response.Value = ProcessObject(jObject, "root", null);
                }
                response.MaxIndex = _count;
                response.ItemsList = _items;
                return response;
            }
            catch(OperationCanceledException ex)
            {
                return response.WithError("Операция отменена.");
            }
            catch (Exception ex)
            {
                return response.WithError(ex.ToString());
            }
        }

        private JsonItem ProcessObject(JObject jObject, string name, JsonItem parent)
        {
            _cts.Token.ThrowIfCancellationRequested();
            var item =
                new JsonItem { Name = name, ItemType = JsonItemType.Object, Index = Interlocked.Increment(ref _count), Parent = parent};
            _items.Add(item);
            if (parent != null)
            {
                parent.Nodes.Add(item);
            }
            foreach (JProperty property in jObject.Properties())
            {
                ProcessToken(property.Value, property.Name, item);
            }
            return item;
        }

        private JsonItem ProcessArray(JArray jArray, string name, JsonItem parent)
        {
            _cts.Token.ThrowIfCancellationRequested();
            var item = new JsonItem { Name = name, ItemType = JsonItemType.Array, Index = Interlocked.Increment(ref _count), Parent = parent };
            _items.Add(item);
            if (parent != null)
            {
                parent.Nodes.Add(item);
            }             
            for (int i = 0; i < jArray.Count; i++)
            {
                var jToken = jArray[i];
                var desk = (jToken is JArray) || (jToken is JValue) ? "" : ": " + jToken?.ToString(Formatting.None);
            
                ProcessToken(jToken, $"[{i}]{desk}", item);
            }
            return item;
        }

        private JsonItem ProcessValue(JValue jValue, string name, JsonItem parent)
        {
            _cts.Token.ThrowIfCancellationRequested();
            var item = new JsonItem { Name = name, Value = jValue.Value?.ToString(), ItemType = JsonItemType.Value, Index = Interlocked.Increment(ref _count), Parent = parent };
            parent.Nodes.Add(item);
            _items.Add(item);
            return item;
        }

        private void ProcessToken(JToken jToken, string name, JsonItem parent)
        {
            if (jToken is JValue jValue)
            {
                ProcessValue(jValue, name, parent);
            }
            else if (jToken is JArray jArray)
            {
                ProcessArray(jArray, name, parent);
            }
            else if (jToken is JObject jObject)
            {
                ProcessObject(jObject, name, parent);
            }
        }
    }
}
