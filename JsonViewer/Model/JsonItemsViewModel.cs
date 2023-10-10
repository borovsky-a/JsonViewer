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
    public sealed partial class JsonItemsViewModel : ObservableObject
    {
        private readonly JsonViewerManager _jsonReaderProcessor;       

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
                View?.GoNext(_jsonReaderProcessor.GetMatchItems());               
            });

        public IRelayCommand GoPrevCommand =>
            new RelayCommand<object>((parameter) =>
            {
                View?.GoPrev(_jsonReaderProcessor.GetMatchItems());
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

        [ObservableProperty]
        private JsonItem? _current;

        [ObservableProperty]
        private string? _filePath;

        [ObservableProperty]
        private string? _error;

        [ObservableProperty]
        private string? _filter;

        [ObservableProperty]
        private JsonItem? _original;

        [ObservableProperty]
        private int _maxIndex;

        [ObservableProperty]
        private ItemsControl? _view;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private int _matchesCount;

        [ObservableProperty]
        private JsonItem? _currentValue;

        [ObservableProperty]
        private JsonItem? _currentPreviewItem;

        public bool CanExecute =>
            View != null && !IsLoading;

        public bool CanNavigate =>
            CanExecute && MatchesCount > 0 && MatchesCount < 1200000 && !string.IsNullOrEmpty(Filter) && !IsError;

        public bool IsError =>
            !string.IsNullOrEmpty(Error);

        private async Task ReadFileCommandExecute(string? refresh)
        {
            var isRefreshCommand = bool.Parse(refresh);
            if (!TryGetFilePath(isRefreshCommand, out var filePath))
            {
                return;
            }

            FilePath = filePath;

            if(string.IsNullOrEmpty(FilePath))
            {
                return;
            }
            IsLoading = true;
            Error = null;
            MatchesCount = 0;
            MaxIndex = 0;
            Original = new JsonItem();

            var response = await
                     _jsonReaderProcessor.ReadJson(FilePath!);

            if (string.IsNullOrEmpty(response.Error))
            {
                Original = response.Value;
            }
            MaxIndex = response.MaxIndex;
            MatchesCount = MaxIndex;
            Error = response.Error;
            if (!IsError && !string.IsNullOrEmpty(Filter))
            {
                await OnFilterExecute();
            }
            IsLoading = false;
        }

        private bool TryGetFilePath(bool refresh, out string? filePath)
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
            filePath = null;
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

            }).ContinueWith(o =>
            {
                IsLoading = false;
                MatchesCount = o.Result;
            }, TaskContinuationOptions.ExecuteSynchronously);
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            switch (e.PropertyName)
            {
                case nameof(IsLoading):
                    {
                        OnPropertyChanged(nameof(CanNavigate));
                        OnPropertyChanged(nameof(CanExecute));
                        break;
                    }
                case nameof(Current):
                    {
                        CurrentValue = new JsonItem { Nodes = Current != null ? new List<JsonItem> { Current } : new List<JsonItem>() };
                        break;
                    }
                case nameof(Error):
                    {
                        OnPropertyChanged(nameof(IsError));
                        break;
                    }
                default:
                    break;
            }
        }
    }
}
