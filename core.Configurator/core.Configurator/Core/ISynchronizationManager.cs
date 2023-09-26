using System.Xml.Linq;

namespace mop.Configurator
{
    public interface ISynchronizationManager
    {
        ConfigurationProvider ConfigurationProvider { get; }
        string Name { get; set; }       
        string DisplayName { get; }
        string SourceInfo { get; }
        string ApplicationName { get; set; }
        XDocument Import(object parameter);    
        void Export(object parameter);
        bool SyncRequired { get; set; }
        bool CanExecute(object obj);
        bool IsCurrent { get; set; }
        void Initialize();
        void SaveSettings(bool saveDocument);
        ISynchronizationManager Clone();
    }
}
