using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JsonViewer.Service;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace JsonViewer.Model
{
    public sealed class JsonItemsViewModel : ObservableObject
    {
        private readonly JsonViewerManager _jsonReaderProcessor;
        private JsonItem _rootItem;
        private JsonItem _currentItem;
        private string _filePath;
        private string _error;
        private string _filter;
        private JsonItem _original;
        private int _maxIndex;
        private bool _showAll = true;
        private ItemsControl _view;
        private bool _isLoading;
        private string _matchesCount;

        public JsonItemsViewModel()
        {
            _rootItem = _original = new JsonItem();
            _jsonReaderProcessor = new JsonViewerManager();
        }

        public IAsyncRelayCommand ReadFileCommand =>
            new AsyncRelayCommand<string>(ReadFileCommandExecute);

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
            OnPropertyChanged(nameof(Root));
        });

        public IRelayCommand GoPrevCommand =>
            new RelayCommand<object>((parameter) =>
        {
            View.GoPrev(Root);
            OnPropertyChanged(nameof(Root));
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

        public bool ShowAll
        {
            get => _showAll;
            set => SetProperty(ref _showAll, value);
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

        public JsonItem Current
        {
            get => _currentItem;
            set => SetProperty(ref _currentItem, value);
        }

        public ItemsControl View
        {
            get => _view;
            set => SetProperty(ref _view, value);
        }

        public bool CanExecute => View != null &&
           !IsLoading;

        public bool CanNavigate =>
            CanExecute &&
            !string.IsNullOrEmpty(Filter);

        private void FilterExecute()
        {
            var response = Original.GetFilteredItem(Root, Filter, ShowAll);
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

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            switch (e.PropertyName)
            {
                case nameof(Original):
                    {
                        Root = Original;
                        if (!string.IsNullOrEmpty(Filter))
                        {
                            FilterExecute();
                        }
                        break;
                    }
                case nameof(Filter):
                case nameof(ShowAll):
                    {
                        if (Original != null)
                        {
                            FilterExecute();
                        }
                        OnPropertyChanged(nameof(CanNavigate));
                        OnPropertyChanged(nameof(CanExecute));
                        break;
                    }
                case nameof(IsLoading):
                    {
                        OnPropertyChanged(nameof(CanNavigate));
                        OnPropertyChanged(nameof(CanExecute));
                        break;
                    }
                default:
                    break;
            }
        }
    }
}
