using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JsonViewer.Service;
using Microsoft.Win32;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace JsonViewer.Model
{
    public sealed class JsonItemsViewModel : ObservableObject
    {
        private int _rootCount;
        private List<JsonItem> _originalItemsList;
        private List<JsonItem> _rootItemsList;
        private readonly JsonViewerManager _jsonReaderProcessor;
        private JsonItem _rootItem;
        private JsonItem _currentItem;
        private string _filePath;
        private string _error;
        private string _filter;
        private JsonItem _original;
        private int _maxIndex;
        private ItemsControl _view;
        private bool _isLoading;
        private string _matchesCount;
        private JsonItem _rootValueItem;
        private JsonItem _currentValue;

        public JsonItemsViewModel()
        {
            _originalItemsList = _rootItemsList = new List<JsonItem>();
            _rootValueItem = new JsonItem();
            _rootItem = _original = new JsonItem();
            _jsonReaderProcessor = new JsonViewerManager();
        }

        public IAsyncRelayCommand ReadFileCommand =>
            new AsyncRelayCommand<string>(ReadFileCommandExecute);

        public IAsyncRelayCommand FilterCommand =>
            new AsyncRelayCommand(() =>
            {
                IsLoading = true;
                return Task.Run(() =>
                {
                    OnFilterExecute();
                    IsLoading = false;
                });
            });

        public IAsyncRelayCommand ExpandCommand =>
            new AsyncRelayCommand(() =>
        {
            IsLoading = true;
            return Task.Run(() =>
            {
                var clone = Root.DeepCopy();
                CollapseCommandExecute(clone, true);
                Root = clone;
                IsLoading = false;
            });
        });

        public IAsyncRelayCommand CollapseCommand =>
            new AsyncRelayCommand(() =>
            {
                IsLoading = true;
                return Task.Run(() =>
                {
                    var clone = Root.DeepCopy();
                    CollapseCommandExecute(clone, false);
                    Root = clone;
                    IsLoading = false;
                });
            });

        public IRelayCommand GoNextCommand =>
            new RelayCommand(() =>
        {
            View.GoNext(_rootItemsList.Where(o => o.IsMatch));
            OnPropertyChanged(nameof(Root));
        });

        public IRelayCommand GoPrevCommand =>
            new RelayCommand<object>((parameter) =>
        {
            View.GoPrev(_rootItemsList.Where(o=> o.IsMatch));
            OnPropertyChanged(nameof(Root));
        });

        public IRelayCommand ClipboardCopyCommand =>
           new RelayCommand<object>((parameter) =>
           {
               if (CurrentValue == null)
               {
                   return;
               }
               var stringValue = CurrentValue.ItemType == JsonItemType.Value ? CurrentValue.Name + ": " + CurrentValue.Value : CurrentValue.Name;
               Clipboard.SetText(stringValue);
           });

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

        public string MatchesCount
        {
            get => _matchesCount;
            set => SetProperty(ref _matchesCount, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
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
        public JsonItem RootValue
        {
            get => _rootValueItem;
            set => SetProperty(ref _rootValueItem, value);
        }
        public JsonItem Current
        {
            get => _currentItem;
            set => SetProperty(ref _currentItem, value);
        }
        public JsonItem CurrentValue
        {
            get => _currentValue;
            set => SetProperty(ref _currentValue, value);
        }

        public ItemsControl View
        {
            get => _view;
            set => SetProperty(ref _view, value);
        }

        public bool CanExecute => View != null &&
           !IsLoading;

        public bool CanNavigate =>
            CanExecute && _rootCount > 0 && _rootCount < 1200000 && !string.IsNullOrEmpty(Filter);

        private void FilterExecute()
        {
            var response = Original.GetFilteredItem(Filter);
            Root = response.Result;
            MatchesCount = response.Matches.Any() ? response.Matches.Count.ToString() : (string.IsNullOrEmpty(Filter) ? "" : "0");
        }

        private async Task ReadFileCommandExecute(string refresh)
        {
            var isRefreshCommand = bool.Parse(refresh);
            if (!TryGetFilePath(isRefreshCommand, out var filePath))
            {
                return;
            }
            IsLoading = true;
            FilePath = filePath;

            await Task.Run(async () =>
            {
                var response = await
                    _jsonReaderProcessor.ReadJson(FilePath).ConfigureAwait(false);
                if (!string.IsNullOrEmpty(response.Error))
                {
                    Error = response.Error;
                }
                else
                {
                    Original = response.Value;
                }
                MaxIndex = response.MaxIndex;
                _originalItemsList = response.ItemsList;
            });
            IsLoading = false;
        }

        private bool TryGetFilePath(bool refresh, out string filePath)
        {
            if (refresh)
            {
                filePath = FilePath;
                return !string.IsNullOrEmpty(filePath);
            }
            var openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                filePath = openFileDialog.FileName;
                return !string.IsNullOrEmpty(filePath);
            }
            filePath = default;
            return false;
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

        private void OnFilterExecute()
        {
            if (Original != null)
            {
                FilterExecute();
            }
            OnPropertyChanged(nameof(CanNavigate));
            OnPropertyChanged(nameof(CanExecute));
        }
        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            switch (e.PropertyName)
            {
                case nameof(Original):
                    {
                        Root = Original;
                        Filter = null;
                        break;
                    }
                case nameof(Root):
                    {
                        if(Root == Original)
                        {
                            _rootCount = _originalItemsList.Count;
                            _rootItemsList = _originalItemsList.ToList();
                        }
                        else
                        {
                            _rootItemsList = Root == null ? new List<JsonItem>() : Root.ToList();
                            _rootCount = _rootItemsList.Count;
                        }                           
                        break;
                    }
                case nameof(IsLoading):
                    {
                        OnPropertyChanged(nameof(CanNavigate));
                        OnPropertyChanged(nameof(CanExecute));
                        break;
                    }
                case nameof(Current):
                    {
                        if (Current == null)
                        {
                            return;
                        }
                        var current = _originalItemsList.FirstOrDefault(o => o.Index == Current.Index);
                        RootValue = new JsonItem { Nodes = new List<JsonItem> { current } };
                        break;
                    }
                default:
                    break;
            }
        }
    }
}
