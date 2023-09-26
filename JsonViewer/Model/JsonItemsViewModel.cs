using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JsonViewer.Service;
using Microsoft.Win32;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

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
        }, CanNavigateCommandExecute);

        public IRelayCommand CollapseCommand =>
            new RelayCommand(() =>
        {
            var clone = Root.DeepCopy();
            CollapseCommandExecute(clone, false);
            Root = clone;
        }, CanNavigateCommandExecute);

        public IRelayCommand GoNextCommand =>
            new RelayCommand(() =>
        {
            View.GoNext(Root);
            OnPropertyChanged(nameof(Root));
        }, CanNavigateCommandExecute);

        public IRelayCommand GoPrevCommand =>
            new RelayCommand(() =>
        {
            View.GoPrev(Root);
            OnPropertyChanged(nameof(Root));
        }, CanNavigateCommandExecute);

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

        private void FilterExecute()
        {
            Root = Original.GetFilteredItem(Root, Filter, ShowAll);
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
        private bool CanNavigateCommandExecute()
        {
            return View != null;
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
                        FilterExecute();
                        break;
                    }
                default:
                    break;
            }
        }
    }
}
