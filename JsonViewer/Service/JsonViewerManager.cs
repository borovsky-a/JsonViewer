using JsonViewer.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace JsonViewer.Service
{
    public sealed class JsonViewerManager
    {
        private int _count;
        private List<JsonItem> _items;
        public async Task<ReaderResponse> ReadJson(string path)
        {
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
                var textValue = File.ReadAllText(path);
                var jsonValue = JToken.Parse(textValue);      
                if(jsonValue is JArray jArray)
                {
                    response.Value = ProcessArray(jArray, "root", null);
                }
                else if(jsonValue is JObject jObject)
                {
                    response.Value = ProcessObject(jObject, "root", null);
                }                       
                response.MaxIndex = _count;
                response.ItemsList = _items;
                return await Task.FromResult(response);
            }
            catch (Exception ex)
            {
                return response.WithError(ex.ToString());
            }
        }

        private JsonItem ProcessObject(JObject jObject, string name, JsonItem parent)
        {
            var item =
                new JsonItem { Name = name, ItemType = JsonItemType.Object, Index = Interlocked.Increment(ref _count) };
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
            var item = new JsonItem { Name = name, ItemType = JsonItemType.Array, Index = Interlocked.Increment(ref _count) };
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
            var item = new JsonItem { Name = name, Value = jValue.Value?.ToString(), ItemType = JsonItemType.Value, Index = Interlocked.Increment(ref _count) };
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
