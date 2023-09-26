using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace mop.Configurator
{
    public class FileSynchronizationManager : BaseViewModel, ISynchronizationManager
    {
        private string _filePath;
        private bool _syncRequired = true;
        public FileSynchronizationManager(ConfigurationProvider configurationProvider)
        {
            ConfigurationProvider = configurationProvider;
        }

        public ICommand EditCommand => new RelayCommand(EditCommandExecute, EditCommandCanExecute);
        protected virtual void EditCommandExecute(object parameter)
        {
            var dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == true)
            {
                FilePath = dialog.FileName;
            }
        }
        protected virtual bool EditCommandCanExecute(object parameter)
        {
            return true;
        }

        public ConfigurationProvider ConfigurationProvider { get; }
        public string Name { get; set; } = "File";
        public string DisplayName => "Файл";
        public string SourceInfo => FilePath;
        public bool IsCurrent { get; set; }
        public string FilePath
        {
            get { return _filePath; }
            set
            {
                if (_filePath != value)
                {
                    _filePath = value;
                    OnPropertyChanged(nameof(FilePath));
                }
            }
        }
        public bool SyncRequired
        {
            get { return _syncRequired; }
            set
            {
                if (_syncRequired != value)
                {
                    _syncRequired = value;
                    OnPropertyChanged(nameof(SyncRequired));
                }
            }
        }

        public string ApplicationName
        {
            get { return _applicationName; }
            set
            {
                if (_applicationName != value)
                {
                    _applicationName = value;
                    OnPropertyChanged(nameof(ApplicationName));
                }
            }
        }

        public bool CanExecute(object obj)
        {
            return !string.IsNullOrEmpty(FilePath);
        }

        public void Export(object parameter)
        {
            try
            {
                var path = FilePath;
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                var dstDir = Path.GetDirectoryName(path);
                if (!Directory.Exists(dstDir))
                {
                    Directory.CreateDirectory(dstDir);
                }
                var config = ConfigurationProvider.XDocument.ToString();
                File.WriteAllText(path, config);
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new ConfiguratorException($"Для записи файла по пути '{FilePath}' недостаточно прав.", ex);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public XDocument Import(object parameter)
        {
            try
            {
                var path = FilePath;
                if (string.IsNullOrEmpty(path))
                    throw new ConfiguratorException("Ошибка при загрузке конфигурации из файла. В качестве пути к файлу передано пустое значение.");
                if (!File.Exists(path))
                    throw new ConfiguratorException("Ошибка при загрузке конфигурации из файла. Файл не найден.");
                using (var reader = XmlReader.Create(path))
                {
                    var doc = XDocument.Load(reader);
                    if (doc == null)
                        throw new ConfiguratorException("Ошибка при загрузке конфигурации из файла. Пустой документ");
                    var applicationName = doc.Root.Attribute("ProgramName")?.Value;
                    if (string.IsNullOrEmpty(applicationName))
                        throw new ConfiguratorException("Ошибка при загрузке конфигурации из файла. Отсутствует имя приложения.");

                    ApplicationName = applicationName;

                    if (SyncRequired)
                    {
                        foreach (var item in ConfigurationProvider.ExportManagers.OfType<FileSynchronizationManager>())
                        {
                            item.FilePath = FilePath;
                        }
                    }
                    return doc;
                }
            }
            catch (ConfiguratorException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при загрузке конфигурации из файла. {ex}", ex);
            }
        }
        private string _applicationName;

        public void Initialize()
        {
            if (!string.IsNullOrEmpty(FilePath))
                return;
            var document = ConfigurationProvider.ConfigurationDocument;
            var parametersSection = document.XPathSelectElement("/Program/Parameters/Parameter[@Name='" + Name + "']");
            if (parametersSection == null)
                return;
            var parameters = parametersSection.Element("Parameters").Elements();
            if (parameters.Count() == 0)
                return;
            var isCurrent = parameters.Where(o => o.Attribute("Name")?.Value == "IsCurrent").FirstOrDefault();
            var syncRequired = parameters.Where(o => o.Attribute("Name")?.Value == "SyncRequired").FirstOrDefault();
            var filePath = parameters.Where(o => o.Attribute("Name")?.Value == "FilePath").FirstOrDefault();
            if (isCurrent != null && int.TryParse(isCurrent.Value, out int isCurrentValue))
            {
                IsCurrent = isCurrentValue == 1;
            }
            if (syncRequired != null && int.TryParse(syncRequired.Value, out int syncRequiredValue))
            {
                SyncRequired = syncRequiredValue == 1;
            }
            if (filePath != null)
            {
                FilePath = filePath.Value;
            }
        }

        public void SaveSettings(bool saveDocument)
        {
            var document = ConfigurationProvider.ConfigurationDocument;
            var parametersSection = document.XPathSelectElement("/Program/Parameters/Parameter[@Name='" + Name + "']");
            if (parametersSection == null)
                return;
            var parameters = parametersSection.Element("Parameters").Elements();
            if (parameters.Count() == 0)
                return;
            var isCurrent = parameters.Where(o => o.Attribute("Name")?.Value == "IsCurrent").FirstOrDefault();
            var syncRequired = parameters.Where(o => o.Attribute("Name")?.Value == "SyncRequired").FirstOrDefault();
            var filePath = parameters.Where(o => o.Attribute("Name")?.Value == "FilePath").FirstOrDefault();
            if (isCurrent != null)
            {
                isCurrent.Value = IsCurrent == true ? "1" : "0";
            }
            if (syncRequired != null)
            {
                syncRequired.Value = SyncRequired == true ? "1" : "0";
            }
            if (filePath != null)
            {
                filePath.Value = FilePath;
            }
            if (saveDocument)
            {
                var configPath = Path.Combine(Path.GetTempPath(), Assembly.GetExecutingAssembly().GetName().Name);
                document.Save(configPath);
            }
        }

        public ISynchronizationManager Clone()
        {
            var result = new FileSynchronizationManager(ConfigurationProvider);
            result.FilePath = FilePath;
            result.Name = Name;
            result.IsCurrent = IsCurrent;
            result.SyncRequired = false;
            result.ApplicationName = ApplicationName;
            return result;
        }
    }
}


