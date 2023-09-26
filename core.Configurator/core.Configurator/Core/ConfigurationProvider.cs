using FT.Common;
using mop.Configurator.Editors;
using mop.Configurator.Log;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace mop.Configurator
{
    public class ConfigurationProvider : BaseViewModel
    {
        public XDocument ConfigurationDocument { get; }
        private ParameterCollection _parameters;
        private string _filter;
        public ConfigurationProvider()
        {
          
            var configPath = Path.Combine(Path.GetTempPath(), Assembly.GetExecutingAssembly().GetName().Name);
            var config = !File.Exists(configPath) ? Properties.Resources.mop_Configurator :  File.ReadAllText(configPath);
            ConfigurationDocument = XDocument.Parse(config);

            CollectionBuilder = new ParameterCollectionBuilder(this);
            var fileImportManager = new FileSynchronizationManager(this) { Name = "FileImportManager" };
            var fileExportManager = new FileSynchronizationManager(this) { Name = "FileExportManager" };
            var fcdImportManager = new TopologyDbSynchronizationManager(this) { Name = "TopologyDBImportManager" };
            var fcdExportManager = new TopologyDbSynchronizationManager(this) { Name = "TopologyDBExportManager" };
            ImportManagers = new ObservableCollection<ISynchronizationManager>();
            ImportManagers.Add(fileImportManager);
            ImportManagers.Add(fcdImportManager);
            ExportManagers = new ObservableCollection<ISynchronizationManager>();
            ExportManagers.Add(fileExportManager);
            ExportManagers.Add(fcdExportManager);
            
            ParameterCache = new Dictionary<string, Parameter>();
            Parameters = new ParameterCollection();
            HiddenElements = new List<string> { "HostType", "Type" };
            Editors = new EditorManager { new StringEditor(), new NumberEditor(), new AccountEditor(), new SqlConnectionEditor(), new PasswordEditor(), new TimeSpanEditor(), new EnumEditor() };
        }

        public ObservableCollection<ISynchronizationManager> ImportManagers { get; }
        public ObservableCollection<ISynchronizationManager> ExportManagers { get; }     
        public IParameterCollectionBuilder CollectionBuilder { get; set; } 
        public IDictionary<string, Parameter> ParameterCache { get; set; }      
        public bool UseCheckBoxes { get; set; }
        public bool UseFilter { get; set; } = true;
        public bool IgnoreFilterFlag { get; set; }
        public List<string> HiddenElements { get; set; }
        public XDocument XDocument { get; set; }       
        public EditorManager Editors { get; set; }
        public ParameterCollection Parameters
        {
            get { return _parameters; }
            set
            {
                if(_parameters != value)
                {
                    _parameters = value;
                    OnPropertyChanged(nameof(Parameters));
                }
            }
        }   
        public string Filer
        {
            get { return _filter; }
            set
            {
                _filter = value;
                OnPropertyChanged(nameof(Filer));
                if (UseFilter)
                    FilterPrameters();
            }
        }

        protected virtual void FilterPrameters()
        {
            IgnoreFilterFlag = false;

            if (string.IsNullOrEmpty(Filer))
            {
                foreach (var item in ParameterCache.Values)
                {
                    if (!item.IsVisible)
                        item.IsVisible = true;
                    else
                    {
                        item.IsNameMatch = false;
                        item.IsValueMatch = false;
                        item.IsDescriptionMatch = false;
                    }                   
                }
            }
            else
            {
                foreach (var item in ParameterCache.Values)
                {
                    item.IsVisible = false;
                    item.IsExpanded = false;
                }
                foreach (var item in ParameterCache.Values)
                {
                    if (item.IsVisible)
                        continue;
                    var isVisible = IsFilterMatch(item);
                    if (!isVisible)
                        continue;
                    item.IsVisible = true;
                    var parent = item.Parent;
                    while (parent != null)
                    {
                        parent.IsExpanded = true;
                        parent.IsVisible = true;
                        parent = parent.Parent;
                    }
                }
                if (!string.IsNullOrEmpty(Filer))
                {
                    var msg = $"Применен фильтр : '{Filer}'";
                    Logger.WriteDebug(this, msg)
                        .WriteUIMessage(this, LogLevel.Debug, msg);
                }
                else
                {
                    var msg = $"Фильтр отменен.";
                    Logger.WriteDebug(this, msg)
                        .WriteUIMessage(this, LogLevel.Debug, msg);
                }
            }
           
            IgnoreFilterFlag = true;
        }

        public virtual bool IsFilterMatch(Parameter item)
        {
            item.IsNameMatch = false;
            item.IsValueMatch = false;
            item.IsDescriptionMatch = false;
            var filter = Filer;
            if (string.IsNullOrEmpty(filter))
            {
                return true;
            }
            else
            {
                var name = string.IsNullOrEmpty(item?.DisplayName) ? "" : item.DisplayName;
                var value = string.IsNullOrEmpty(item?.Value) ? "" : item.Value;
                var description = string.IsNullOrEmpty(item?.Description) ? "" : item.Description;
                if (CultureInfo.CurrentCulture.CompareInfo.IndexOf(name, filter, CompareOptions.IgnoreCase) >= 0)
                {
                    item.IsNameMatch = true;
                    return true;
                }
                if (CultureInfo.CurrentCulture.CompareInfo.IndexOf(value, filter, CompareOptions.IgnoreCase) >= 0)
                {
                    item.IsValueMatch = true;
                    return true;
                }
                if (CultureInfo.CurrentCulture.CompareInfo.IndexOf(description, filter, CompareOptions.IgnoreCase) >= 0)
                {
                    item.IsDescriptionMatch = true;
                    return true;
                }
            }
            return false;
        }

        public virtual bool Refresh(ISynchronizationManager synchronizationManager)
        {
            try
            {
                Logger.WriteDebug(this, "Выполняется попытка обновления конфигурации.")
                      .WriteUIMessage(this, LogLevel.Debug, "Выполняется попытка обновления конфигурации.");
              
                if(synchronizationManager == null)
                {
                    Logger.WriteWarning(this, "Не выбран провайдер для импорта конфигурации. Действие отменено.")
                    .WriteUIMessage(this, LogLevel.Warning, "Не выбран провайдер для импорта конфигурации. Действие отменено.");
                    return false;
                }
                foreach (var item in ParameterCache)
                {
                    item.Value.PropertyChanged -= Parameter_PropertyChanged;
                }
                var doc = synchronizationManager.Import(null);
                var parameters = CollectionBuilder.Create(doc);               
                if(parameters != null)
                {
                    var cache = parameters.ToDictionary();
                    ParameterCache = cache;
                }
                XDocument = doc;
                Parameters = parameters;
                Logger.WriteInformation(this, $"Настройки Загружены. Источник : {synchronizationManager.SourceInfo}")
                    .WriteUIMessage(this, LogLevel.Information, $"Настройки Загружены. Источник : {synchronizationManager.SourceInfo}");
                if(!string.IsNullOrEmpty(Filer))
                    FilterPrameters();
                return true;
            }
            catch (ConfiguratorException ex)
            {
                Logger.WriteError(this, ex.ToLogString())
                      .WriteUIMessage(this, LogLevel.Error, ex.Message);
            }
            catch (Exception ex)
            {
                var msg = "Во время обновления конфигурации произошла ошибка. {0}";
                Logger.WriteError(this, string.Format(msg, ex.ToLogString()) )
                     .WriteUIMessage(this, LogLevel.Error, string.Format(msg, ex.Message));
            }
            return false;
        }
       
        public virtual bool Save(ISynchronizationManager synchronizationManager)
        {
            try
            {
                Logger.WriteDebug(this, "Выполняется попытка сохранения конфигурации.")
                     .WriteUIMessage(this, LogLevel.Debug, "Выполняется попытка сохранения конфигурации.");
              
                if(synchronizationManager == null)
                {
                    Logger.WriteWarning(this, "Не выбран провайдер для экспорта конфигурации. Действие отменено.")
                     .WriteUIMessage(this, LogLevel.Warning, "Не выбран провайдер для экспорта конфигурации. Действие отменено.");
                    return false;
                }
                Save((Parameter)null);
                synchronizationManager.Export(null);
                Logger.WriteInformation(this, $"Настройки сохранены. Источник: {synchronizationManager.SourceInfo}")
                      .WriteUIMessage(this, LogLevel.Information, $"Настройки сохранены. Источник: {synchronizationManager.SourceInfo}");
                return true;
            }
            catch (ConfiguratorException ex)
            {
                Logger.WriteError(this, ex.ToLogString())
                      .WriteUIMessage(this, LogLevel.Error, ex.Message);
            }
            catch (Exception ex)
            {
                var msg = $"Во время сохранения документа произошла ошибка. Источник: {synchronizationManager.SourceInfo} Ошибка: {ex}";
                Logger.WriteError(this, msg)
                     .WriteUIMessage(this, LogLevel.Error, msg);
            }
            return false;
        }

        protected virtual void Save(Parameter parameter)
        {
            var localParameters = parameter == null ? Parameters : parameter.Parameters;
            if (parameter != null)
                parameter.Save();
            foreach (var item in localParameters)
            {
                Save(item);
            }
        }

        public void Parameter_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var parameter = (Parameter)sender;
            if (e.PropertyName == nameof(parameter.Value))
            {
                var oldValue = parameter.OldValue;
                var newValue = parameter.Value;
                if (!string.IsNullOrEmpty(oldValue))
                {
                    oldValue = oldValue.Split(Environment.NewLine.ToArray()).FirstOrDefault();
                }
                if (!string.IsNullOrEmpty(newValue))
                {
                    newValue = newValue.Split(Environment.NewLine.ToArray()).FirstOrDefault();
                }
                var m = $"Изменено значение {parameter.Parent?.DisplayName} с '{oldValue}' на '{newValue}'";
                Logger.WriteDebug(parameter, m)
                   .WriteUIMessage(parameter, LogLevel.Debug, m);
            };
        }     
    }
}
