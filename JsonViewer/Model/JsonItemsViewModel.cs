using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JsonViewer.Service;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Xml.Linq;

namespace JsonViewer.Model
{
    public sealed class JsonItemsViewModel : ObservableObject
    {
        private readonly JsonViewerManager _jsonReaderProcessor;
        private JsonItem _rootItem;
        private string _filePath;
        private string _error;
        private string _filter;
        private JsonItem _original;
        private int _maxIndex;
        private bool _showAll;
        private ItemsControl _view;

        public JsonItemsViewModel()
        {
            _rootItem = _original = new JsonItem();
            _jsonReaderProcessor = new JsonViewerManager();
        }

        public IAsyncRelayCommand ReadFileCommand => 
            new AsyncRelayCommand(ReadFileCommandExecute);             

        public IRelayCommand ExpandCommand =>
            new RelayCommand(() =>
        {
            var clone = Root.DeepCopy();
            CollapseCommandExecute(clone, true);
            Root = clone;
        });

        public IRelayCommand CollapseCommand =>
            new RelayCommand(() =>
        {
            var clone = Root.DeepCopy();
            CollapseCommandExecute(clone, false);
            Root = clone;
        });

        public IRelayCommand GoNextCommand =>
            new RelayCommand(() =>
        {
            View.GoNext(Root);

        }, () => View != null);

        public IRelayCommand GoPrevCommand =>
            new RelayCommand(() =>
        {
            View.GoPrev(Root);

        }, () => View != null);

        public string Filter
        {
            get => _filter;
            set => SetProperty(ref _filter, value);
        }

        public string Error
        {
            get => _error;
            set => SetProperty(ref _error, value);
        }

        public string FilePath
        {
            get => _filePath;
            set => SetProperty(ref _filePath, value);
        }
        public int MaxIndex
        {
            get => _maxIndex;
            set => SetProperty(ref _maxIndex, value);
        }
        public bool ShowAll
        {
            get => _showAll;
            set => SetProperty(ref _showAll, value);
        }
        public JsonItem Original
        {
            get => _original;
            set => SetProperty(ref _original, value);
        }

        public JsonItem Root
        {
            get => _rootItem;
            set => SetProperty(ref _rootItem, value);
        }

        public ItemsControl View
        {
            get => _view;
            set => SetProperty(ref _view, value);
        }

        private void ExecuteFilter()
        {
            if (string.IsNullOrEmpty(Filter))
            {
                var clone = Original.DeepCopy();
                var filteredList = Root.ToList().Where(o => o.IsMatch);
                var oroginalsList = clone.ToList();
                foreach (var filteredItem in filteredList)
                {
                    var oItem = oroginalsList.FirstOrDefault(o => o.Index == filteredItem.Index);
                    if (oItem != null)
                    {
                        SetParentsState(oItem, (o) =>
                        {
                            o.IsVisible = true;
                            o.IsExpanded = true;
                        });
                    }
                    oItem.IsExpanded = false;
                }
                Root = clone;
            }
            else
            {
                Root = GetFilteredItem(Original);
            }
        }

        private JsonItem GetFilteredItem(JsonItem root)
        {
            var clone = root.DeepCopy();
            PrepareFilterItems(clone);
            FilterItems(clone);
            return clone;
        }

        private void PrepareFilterItems(JsonItem root)
        {
            if (!string.IsNullOrEmpty(root.Name) && (root.Name.ContainsIgnoreCase(Filter) || root.Value.ContainsIgnoreCase(Filter)))
            {
                root.IsMatch = true;
                SetParentsState(root, (o) =>
                {
                    o.IsVisible = true;
                    o.IsExpanded = true;
                });
                root.IsExpanded = false;
            }
            else
            {
                root.IsVisible = false;
            }
            if (root.Name == "root")
            {
                root.IsVisible = true;
            }
            foreach (var node in root.Nodes)
            {
                node.IsMatch = node.Name.ContainsIgnoreCase(Filter) || root.Value.ContainsIgnoreCase(Filter);
                if (node.IsMatch)
                {
                    SetParentsState(node, (o) =>
                    {
                        o.IsVisible = true;
                        o.IsExpanded = true;
                    });
                    node.IsExpanded = false;
                }
                PrepareFilterItems(node);
            }
        }

        private void SetParentsState(JsonItem node, Action<JsonItem> action)
        {
            action(node);
            var parent = node.Parent;
            while (parent != null)
            {
                action(parent);
                parent = parent.Parent;
            }
        }

        private void FilterItems(JsonItem root)
        {
            if (!root.IsVisible)
            {
                if (!ShowAll)
                {
                    root.Nodes.Clear();
                }
                return;
            }

            var nodes = ShowAll ? root.Nodes : root.Nodes.Where(o => o.IsVisible).ToList();
            foreach (var node in nodes)
            {
                FilterItems(node);
            }
            root.Nodes = nodes;
        }
        private async Task ReadFileCommandExecute()
        {
            var openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                FilePath = openFileDialog.FileName;
                var response = await _jsonReaderProcessor.ReadJson(FilePath);
                if (!string.IsNullOrEmpty(response.Error))
                {
                    Error = response.Error;
                }
                else
                {
                    Original = response.Value;
                }
                MaxIndex = response.MaxIndex;
            }
        }
        private void CollapseCommandExecute(JsonItem root, bool expand)
        {
            root.IsExpanded = expand;
            if (!root.Nodes.Any())
            {
                return;
            }
            foreach (var node in root.Nodes)
            {
                node.IsExpanded = expand;
                CollapseCommandExecute(node, expand);
            }
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            switch (e.PropertyName)
            {
                case nameof(Original):
                    {
                        Root = Original;
                        Filter = "";
                        break;
                    }
                case nameof(Filter):
                case nameof(ShowAll):
                    {
                        ExecuteFilter();
                        break;
                    }
                default:
                    break;
            }
        }
    }
}
