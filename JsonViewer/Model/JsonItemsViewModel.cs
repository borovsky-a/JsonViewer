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
        private readonly JsonViewerManager _jsonReaderProcessor;
        private JsonItem _currentItem;
        private string _filePath;
        private string _error;
        private string _filter;
        private JsonItem _original;
        private int _maxIndex;
        private ItemsControl _view;
        private bool _isLoading;
        private int _matchesCount;
        private JsonItem _currentValue;
        private JsonItem _currentPreviewItem;

        public JsonItemsViewModel()
        {
            _jsonReaderProcessor = new JsonViewerManager();
        }

        public IAsyncRelayCommand ReadFileCommand =>
            new AsyncRelayCommand<string>(ReadFileCommandExecute);

        public IAsyncRelayCommand FilterCommand =>
            new AsyncRelayCommand(() =>
            {
                return OnFilterExecute();
            });

        public IAsyncRelayCommand ExpandCommand =>
            new AsyncRelayCommand(() =>
        {
            IsLoading = true;
            return Task.Run(() =>
            {
                _jsonReaderProcessor.ExpandItems();
                IsLoading = false;
            });
        });

        public IAsyncRelayCommand CollapseCommand =>
            new AsyncRelayCommand(() =>
            {
                IsLoading = true;
                return Task.Run(() =>
                {
                    _jsonReaderProcessor.CollapseItems();
                    IsLoading = false;
                });
            });

        public IRelayCommand GoNextCommand =>
            new RelayCommand(() =>
            {
                View.GoNext(_jsonReaderProcessor.GetMatchItems());
            });

        public IRelayCommand GoPrevCommand =>
            new RelayCommand<object>((parameter) =>
            {
                View.GoPrev(_jsonReaderProcessor.GetMatchItems());
            });

        public IRelayCommand ClipboardCopyCommand =>
           new RelayCommand<object>((parameter) =>
           {
               if (CurrentPreviewItem == null)
               {
                   return;
               }
               var stringValue = CurrentPreviewItem.GetDisplayValue();
               Clipboard.SetText(stringValue);
           });

        public IRelayCommand CancelReadFileCommand =>
            new RelayCommand(() =>
            {
                _jsonReaderProcessor.CancelReadFileCommand();
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

        public int MatchesCount
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

        public JsonItem CurrentPreviewItem
        {
            get => _currentPreviewItem;
            set => SetProperty(ref _currentPreviewItem, value);
        }

        public ItemsControl View
        {
            get => _view;
            set => SetProperty(ref _view, value);
        }

        public bool CanExecute => View != null &&
           !IsLoading;

        public bool CanNavigate =>
            CanExecute && _matchesCount > 0 && _matchesCount < 1200000 && !string.IsNullOrEmpty(Filter);      

        private async Task ReadFileCommandExecute(string refresh)
        {
            var isRefreshCommand = bool.Parse(refresh);
            if (!TryGetFilePath(isRefreshCommand, out var filePath))
            {
                return;
            }
            IsLoading = true;
            FilePath = filePath;
            MatchesCount = 0;
            MaxIndex = 0;
            Original = new JsonItem();
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
                MatchesCount = MaxIndex;
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

        private Task OnFilterExecute()
        {
            if (Original == null || !Original.Nodes.Any())
            {
                return Task.CompletedTask;
            }
            IsLoading = true;
            return Task.Run(() =>
            {
               var matchesCount = _jsonReaderProcessor.FilteredItems(Filter);
               return matchesCount;

            }).ContinueWith(o=> {
                IsLoading = false;
                MatchesCount = o.Result;
            }, TaskContinuationOptions.ExecuteSynchronously);
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            switch (e.PropertyName)
            {
                case nameof(Original):
                    {
                        Filter = null;
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
                        CurrentValue = new JsonItem { Nodes = new List<JsonItem> { Current } };
                        break;
                    }
                default:
                    break;
            }
        }
    }
}
