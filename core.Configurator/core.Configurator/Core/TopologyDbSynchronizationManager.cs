using FT.Common;
using mop.Configurator.Editors.DataConnection;
using mop.Configurator.Log;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using System.Xml.Linq;
using System.Xml.XPath;

namespace mop.Configurator
{
    public  class TopologyDbSynchronizationManager : BaseViewModel, ISynchronizationManager
    {
        private IEnumerable<VConfiguration> _configuration;
        private IEnumerable<VConfiguration> _entities;
        private bool _syncRequired = true;
        private string _connectionString;
        private string _host;
        private string _applicationName;
        private string _programVersionInstanceCode;
        public TopologyDbSynchronizationManager(ConfigurationProvider configurationProvider, bool refresh = true)
        {

#warning To protect potentially sensitive information in your connection string, you should move it out of source code. 
#if DEBUG
            //ConnectionString = "Server=.\\SQL2019SRV;Database=TopologyDB;Integrated Security=True;Trusted_Connection=True;";
#endif

            Entities = new ObservableCollection<VConfiguration>();
            ConfigurationProvider = configurationProvider;
            if(refresh)
                RefresCommandExecute(false);
        }
        public ConfigurationProvider ConfigurationProvider { get; }
        public string Name { get; set; } ="InfrastructureDb";
        public string DisplayName => "База данных (Topology)";
        public IEnumerable<VConfiguration> Entities 
        {
            get { return _entities; }
            set
            {
                if(_entities != value)
                {
                    _entities = value;
                    OnPropertyChanged(nameof(Entities));
                }
            }
        }
        public ICommand EditCommand => new RelayCommand(EditCommandExecute, EditCommandCanExecute);
        public ICommand RefreshCommand => new RelayCommand(RefresCommandExecute, RefreshCommandCanExecute);
        protected virtual void EditCommandExecute(object parameter)
        {
            try
            {
                string connectionString = null;
                try
                {
                    connectionString = new SqlConnectionStringBuilder(ConnectionString) { IntegratedSecurity = true }.ConnectionString;
                }
                catch (Exception)
                {
                    connectionString = null;
                }
                using (var dialog = new DataConnectionDialog())
                {
                    if (string.IsNullOrEmpty(connectionString))
                        dialog.ConnectionString = new SqlConnectionStringBuilder { IntegratedSecurity = true }.ConnectionString;
                    else
                        dialog.ConnectionString = connectionString;
                    if (DataConnectionDialog.Show(dialog) == System.Windows.Forms.DialogResult.OK)
                    {
                        ConnectionString = dialog.ConnectionString;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteError(this, $"Ошибка при создании строки подключения. {ex.ToLogString()}")
                    .WriteUIMessage(this, LogLevel.Error, $"Ошибка при создании строки подключения. {ex.ToLogString()}");
            }
        }
        protected virtual bool EditCommandCanExecute(object parameter)
        {
            return true;
        }
        private bool RefreshCommandCanExecute(object arg)
        {
            return !string.IsNullOrEmpty(ConnectionString);
        }

        private void RefresCommandExecute(object parameter)
        {
            try
            {
                bool refresh = false;
                if (parameter == null)
                    parameter = "true";
                if (!bool.TryParse(parameter.ToString(), out refresh))
                {
                    refresh = true;
                }
                Entities = GetConfiguration(refresh);
                OnPropertyChanged(nameof(Entities));
            }
            catch (Exception ex)
            {
                if (Entities == null)
                    Entities = new List<VConfiguration>();
                Logger.WriteError(this, $"Ошибка при обновлении данных. {ex.ToLogString()}")
                    .WriteUIMessage(this, LogLevel.Error, $"Ошибка при обновлении данных. {ex.ToLogString()}");
            }
        }
        public IEnumerable<string> Hosts => Entities.Select(o => o.HostName).Distinct().OrderBy(o=> o);
        public IEnumerable<string> ApplicationNames => Entities.Where(o => o.HostName == Host).Select(o => o.ProgramName).Distinct().OrderBy(o => o);
        public IEnumerable<string> ProgramVersionInstanceCodes => Entities.Where(o => o.ProgramName == ApplicationName && o.HostName == Host).Select(o => o.ProgramVersionInstanceCode).Distinct().OrderBy(o => o);
        public string SourceInfo
        {
            get
            {
                if (string.IsNullOrEmpty(ConnectionString))
                    return "Нет данных";
                try
                {
                    var builder = new SqlConnectionStringBuilder(ConnectionString);
                    return "DataSource: " + builder.DataSource + " InitialCatalog: " + builder.InitialCatalog;
                }
                catch
                {
                    return "Нет данных";
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
       

        public string ConnectionString
        {
            get { return _connectionString; }
            set
            {
                if (_connectionString != value)
                {
                    _connectionString = value;
                    OnPropertyChanged(nameof(ConnectionString));
                }
            }
        }
        public string Host
        {
            get { return _host; }
            set
            {
                if (_host != value)
                {
                    _host = value;
                    OnPropertyChanged(nameof(Host));
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
        public string Version
        {
            get
            {
                var version = Entities.Where(o => o.HostName == Host &&
                    o.ProgramName == ApplicationName &&
                    o.ProgramVersionInstanceCode == ProgramVersionInstanceCode)
                .Select(o => o.ProgramVersion).FirstOrDefault();
                return version;
            }
        }
        public string ProgramVersionInstanceCode
        {
            get { return _programVersionInstanceCode == null ? "" : _programVersionInstanceCode; }
            set
            {
                if (_programVersionInstanceCode != value)
                {
                    _programVersionInstanceCode = value;
                    OnPropertyChanged(nameof(ProgramVersionInstanceCode));
                }
            }
        }
        public bool IsCurrent { get; set; } = true;

        public void Export(object obj)
        {
            if (string.IsNullOrEmpty(ConnectionString))
                throw new ConfiguratorException("При получении информации о конфигурации приложений произошла ошибка. В качестве строки подключения задано пустое значение");
            if (string.IsNullOrEmpty(ApplicationName))
                throw new ConfiguratorException("При получении информации о конфигурации приложений произошла ошибка. В качестве имени приложения задано пустое значение");
            if (string.IsNullOrEmpty(Host))
                throw new ConfiguratorException("При получении информации о конфигурации приложений произошла ошибка. В качестве хоста приложения задано пустое значение");
            if (string.IsNullOrEmpty(Version))
                throw new ConfiguratorException("При получении информации о конфигурации приложений произошла ошибка. В качестве версии приложения задано пустое значение");
            if (ProgramVersionInstanceCode == null)
                throw new ConfiguratorException("При получении информации о конфигурации приложений произошла ошибка. В качестве кода приложения задано пустое значение");
            var doc = ConfigurationProvider.XDocument;
         
            using (var connection = new SqlConnection(ConnectionString))
            {            
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        var existsVersion = GetExistsVersion(ApplicationName, Host, ProgramVersionInstanceCode);
                        var isVersionEquals = existsVersion == Version.ToString();
                        if (!string.IsNullOrEmpty(existsVersion) && !isVersionEquals)
                        {
                            TryUnregisterServiceVersion(
                                        ApplicationName,
                                        Host,
                                        new Version(existsVersion),
                                        ProgramVersionInstanceCode,
                                        connection,
                                        transaction);
                        }
                        TryRegisterServiceVersion(
                            ApplicationName, 
                            Host,
                            new Version(Version),
                            ProgramVersionInstanceCode,
                            doc.ToString(),
                            true,
                            connection, 
                            transaction);
                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }                  
                }
            }
        }

        public XDocument Import(object obj)
        {
            if (string.IsNullOrEmpty(ConnectionString))
                throw new ConfiguratorException("При получении информации о конфигурации приложений произошла ошибка. В качестве строки подключения задано пустое значение");
            if (string.IsNullOrEmpty(ApplicationName))
                throw new ConfiguratorException("При получении информации о конфигурации приложений произошла ошибка. В качестве имени приложения задано пустое значение");
            if (string.IsNullOrEmpty(Host))
                throw new ConfiguratorException("При получении информации о конфигурации приложений произошла ошибка. В качестве хоста приложения задано пустое значение");
            if (string.IsNullOrEmpty(Version))
                throw new ConfiguratorException("При получении информации о конфигурации приложений произошла ошибка. В качестве версии приложения задано пустое значение");
            if (ProgramVersionInstanceCode == null)
                throw new ConfiguratorException("При получении информации о конфигурации приложений произошла ошибка. В качестве кода приложения задано пустое значение");
            var doc = new XDocument();
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                var programInfo = GetProgramInfo(ApplicationName, Version.ToString(), Host, ProgramVersionInstanceCode, connection);
                var parametersList = GetParamValuesFromDataBase(ApplicationName, Host, new Version(Version), ProgramVersionInstanceCode, connection);
                var programHostTypes = GetHostTypeProgram(ApplicationName, connection);

                var root = new XElement("Program");
                var rootProgramName = new XAttribute("ProgramName", programInfo.ProgramName);
                var rootDescription = new XAttribute("Description", programInfo.ProgramTypeDescription);
                var rootDefaultConfigurationName = new XAttribute("DefaultConfigurationName", programInfo.ConfigurationName);
                var rootProgramTypeName = new XAttribute("ProgramTypeName", programInfo.ProgramTypeName);
                var rootIgnoreDefaultConfiguration = new XAttribute("IgnoreDefaultConfiguration", "true");
                root.Add(rootProgramName);
                root.Add(rootDescription);
                root.Add(rootDefaultConfigurationName);
                root.Add(rootProgramTypeName);
                root.Add(rootIgnoreDefaultConfiguration);

                if(programHostTypes.Count() > 0)
                {
                    var hostTypes = new XElement("HostTypes");
                    foreach (var item in programHostTypes)
                    {
                        hostTypes.Add(new XElement("HostType", new XAttribute("Name", item)));
                    }
                    root.Add(hostTypes);
                }
                
                var parameterTypes = new XElement("Types");
                foreach (var parameter in parametersList)
                {
                    if (string.IsNullOrEmpty(parameter.SerializationType))
                        continue;

                    if (parameterTypes.Elements("Type").Where(o => o.Attribute("Name")?.Value == parameter.ValueTypeName).Any())
                        continue;

                    var parameterType = new XElement("Type");
                    var name = new XAttribute("Name", parameter.ValueTypeName);
                    var storageTypeName = new XAttribute("StorageTypeName", parameter.StorageTypeName ?? "");
                    var storageLenght = new XAttribute("StorageLenght", -1);
                    var sQLTypeName = new XAttribute("SQLTypeName", parameter.SQLTypeName ?? "");
                    var cSTypeName = new XAttribute("CSTypeName", parameter.CSTypeName ?? "");
                    var delphiTypeName = new XAttribute("DelphiTypeName", "dtp");
                    var serializationType = new XAttribute("SerializationType", parameter.SerializationType ?? "");
                    parameterType.Add(name);
                    parameterType.Add(storageTypeName);
                    parameterType.Add(storageLenght);
                    parameterType.Add(sQLTypeName);
                    parameterType.Add(cSTypeName);
                    parameterType.Add(delphiTypeName);
                    parameterType.Add(serializationType);
                    if (!string.IsNullOrEmpty(parameter.CheckExpression))
                    {
                        var expr = new XElement("CheckExpression");
                        expr.Value = parameter.CheckExpression;
                        parameterType.Add(expr);
                    }
                    parameterTypes.Add(parameterType);
                }

                
                root.Add(parameterTypes);

                var parametersEl = new XElement("Parameters");
                root.Add(parametersEl);

                if (parametersList.Any())
                {
                    foreach (var parameter in parametersList)
                    {
                        var parameterFullName = parameter.ParameterFullName;
                        if (string.IsNullOrEmpty(parameterFullName))
                            continue;

                        var tmlpLilt = new List<ParamValues>();
                        var currentTmpParameter = parameter;

                        while (currentTmpParameter != null)
                        {
                            tmlpLilt.Add(currentTmpParameter);
                            if (currentTmpParameter.ParentParameterID.HasValue)
                            {
                                currentTmpParameter = parametersList.FirstOrDefault(o => o.ParameterID == currentTmpParameter.ParentParameterID.Value);
                            }
                            else
                            {
                                currentTmpParameter = null;
                            }
                        }

                        var currentParameterEl = parametersEl;
                        for (int i = tmlpLilt.Count - 1; i >= 0; i--)
                        {
                            var currentParameter = tmlpLilt[i];
                            var p_element = currentParameterEl.Descendants("Parameter").FirstOrDefault(o => o.Attribute("Name")?.Value == currentParameter.ParameterName);
                            if (p_element == null)
                            {
                                p_element = new XElement("Parameter");
                                currentParameterEl.Add(p_element);

                                var hasChildren = parametersList.Any(o => o.ParentParameterID == currentParameter.ParameterID);

                                var name = new XAttribute("Name", currentParameter.ParameterName);
                                p_element.Add(name);
                                var valueTypeName = new XAttribute("ValueTypeName", currentParameter.ValueTypeName);
                                p_element.Add(valueTypeName);
                                var isRequired = new XAttribute("IsRequired", currentParameter.IsRequired);
                                p_element.Add(isRequired);
                                var isVisible = new XAttribute("IsVisible", currentParameter.IsVisible);
                                p_element.Add(isVisible);
                                var isReadOnly = new XAttribute("IsReadOnly", currentParameter.IsReadOnly);
                                p_element.Add(isReadOnly);
                                var description = new XAttribute("Description", currentParameter.Description);
                                p_element.Add(description);

                                if (hasChildren)
                                {
                                    var chParametersEl = new XElement("Parameters");
                                    p_element.Add(chParametersEl);
                                    currentParameterEl = chParametersEl;
                                }
                                else
                                {
                                    switch (currentParameter.StorageTypeID)
                                    {
                                        case 1:
                                            p_element.Add(new XElement("DefaultIntValue", currentParameter.DefaultIntValue));
                                            p_element.Add(new XElement("IntValue", currentParameter.IntValue));
                                            break;
                                        case 2:
                                            p_element.Add(new XElement("DefaultDateTimeValue", currentParameter.DefaultDateTimeValue));
                                            p_element.Add(new XElement("DateTimeValue", currentParameter.DateTimeValue));
                                            break;
                                        case 3:
                                            p_element.Add(new XElement("DefaultStringValue", currentParameter.DefaultStringValue));
                                            p_element.Add(new XElement("StringValue", currentParameter.StringValue));
                                            break;
                                        default:
                                            break;
                                    }
                                }
                            }
                            else
                            {
                                currentParameterEl = p_element.Element("Parameters");
                            }
                        }
                    }
                }
                doc.Add(root);
            }
            if (SyncRequired)
            {
                foreach (var item in ConfigurationProvider.ExportManagers.OfType<TopologyDbSynchronizationManager>())
                {
                    item.ConnectionString = ConnectionString;
                    item.Entities = Entities;
                    item.Host = Host;
                    item.ApplicationName = ApplicationName;
                    item.ProgramVersionInstanceCode = ProgramVersionInstanceCode;
                }
            }
            return doc;
        }

        /// <summary>
        ///     Получает версию приложения, если оно зарегистрировано
        /// </summary>
        /// <param name="applicationName"></param>
        /// <param name="hostName"></param>
        /// <param name="instanceCode"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public string GetExistsVersion(string applicationName, string hostName, string instanceCode)
        {
            if (instanceCode == null)
                instanceCode = "";
            var commandString =
@"SELECT [ProgramVersion]     
    FROM [lan].[vProgramVersionInstance]   
   WHERE [ProgramName] =  @ProgramName
  	 AND [HostName]= @HostName
     AND [Code] = @Code
	 AND [IsDeleted] = 0";
            try
            {
                string result = null;
                using (var connection = new SqlConnection(_connectionString))
                using (var command = new SqlCommand(commandString, connection))
                {
                    connection.Open();
                    command.CommandType = CommandType.Text;
                    command.Parameters.Add("@HostName", SqlDbType.NVarChar, 128).Value = hostName;
                    command.Parameters.Add("@ProgramName", SqlDbType.NVarChar, 128).Value = applicationName;
                    command.Parameters.Add("@Code", SqlDbType.VarChar, 100).Value = instanceCode;
                    using (var reader = command.ExecuteReader())
                    {
                        if (!reader.HasRows)
                            return null;
                        while (reader.Read())
                        {
                            result = reader["ProgramVersion"] == null ? null : reader[0].ToString();
                        }
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                string error = $"Во время попытки получения статуса регистрации приложения произошла ошибка. ApplicationName: {applicationName} HostName: {hostName} ProgramVersionInstanceCode: {""} Exception: {ex}";
                throw new ConfiguratorException(error, ex);
            }
        }
        /// <summary>
        ///     Отменяет регистрацию сервиса в базе данных
        /// </summary>       
        private void TryUnregisterServiceVersion(string applicationName, string hostName, Version version, string programVersionInstanceCode, SqlConnection connection, SqlTransaction transaction)
        {
            try
            {
                var commandString = "[lan].[DeleteProgramVersionInstance]";
                using (var command = new SqlCommand(commandString, connection, transaction))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("@HostName", SqlDbType.NVarChar, 128).Value = hostName;
                    command.Parameters.Add("@ProgramName", SqlDbType.NVarChar, 128).Value = applicationName;
                    command.Parameters.Add("@ProgramVersion", SqlDbType.NVarChar, 128).Value = version.ToString();
                    command.Parameters.Add("@ProgramVersionInstanceCode", SqlDbType.VarChar, 255).Value = programVersionInstanceCode;
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                var error = $"Во время отмены регистрации приложения в базе данных произошла ошибка.  ApplicationName: {applicationName} HostName: {hostName} ProgramVersion: {version} ProgramVersionInstanceCode: {programVersionInstanceCode} Exception: {ex.Message}";
                throw new ConfiguratorException(error, ex);
            }
        }
        private void TryRegisterServiceVersion(string applicationName, string hostName, Version version, string programVersionInstanceCode, string settingsParameter, bool forceUpdate, SqlConnection connection, SqlTransaction transaction)
        {
            var commandString = "[lan].[CreateDefaultConfiguration]";

            using (var command = new SqlCommand(commandString, connection, transaction))
            {

                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add("@HostName", SqlDbType.NVarChar, 128).Value = hostName;
                command.Parameters.Add("@Configuration", SqlDbType.Xml).Value = settingsParameter;
                command.Parameters.Add("@ProgramVersion", SqlDbType.VarChar, 255).Value = version.ToString();
                command.Parameters.Add("@ProgramVersionInstanceCode", SqlDbType.VarChar, 255).Value = programVersionInstanceCode;

                if (forceUpdate)
                {
                    command.Parameters.Add("@AlwaysRecreateNewConfiguration", SqlDbType.Bit).Value = forceUpdate;
                }

                command.ExecuteNonQuery();
            }
        }
        public IEnumerable<VConfiguration> GetConfiguration(bool refresh)
        {
            if (string.IsNullOrEmpty(ConnectionString))
                throw new ConfiguratorException("При получении информации о конфигурации приложений произошла ошибка. В качестве строки подключения передано пустое значение");
            if (refresh == false && _configuration != null)
            {
                return _configuration;
            }
            var commandSring =
 @"SELECT  [ProgramName]
      ,[ProgramVersion]
      ,[HostName]
      ,[ConfigurationID]
      ,[ChangeDate]
      ,[ProgramVersionInstanceCode]
      ,[ProgramID]
      ,[ProgramVersionID]
      ,[ProgramVersionInstanceID]
  FROM [lan].[vConfiguration]";

            var result = new List<VConfiguration>();
            using (var connection = new SqlConnection(ConnectionString))
            using (var commnad = connection.CreateCommand())
            {
                commnad.CommandType = System.Data.CommandType.Text;
                commnad.CommandText = commandSring;
                connection.Open();
                using (var reader = commnad.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        var fieldSet = new Dictionary<string, int>();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            fieldSet[reader.GetName(i)] = i;
                        }
                        while (reader.Read())
                        {
                            var entity = new VConfiguration();
                            entity.ProgramName = reader.IsDBNull(fieldSet[nameof(entity.ProgramName)]) ? null : reader.GetString(fieldSet[nameof(entity.ProgramName)]);
                            entity.ProgramVersion = reader.IsDBNull(fieldSet[nameof(entity.ProgramVersion)]) ? null : reader.GetString(fieldSet[nameof(entity.ProgramVersion)]);
                            entity.HostName = reader.IsDBNull(fieldSet[nameof(entity.HostName)]) ? null : reader.GetString(fieldSet[nameof(entity.HostName)]);
                            entity.ConfigurationID = reader.GetInt32(fieldSet[nameof(entity.ConfigurationID)]);
                            entity.ChangeDate = reader.GetDateTimeOffset(fieldSet[nameof(entity.ChangeDate)]);
                            entity.ProgramVersionInstanceCode = reader.IsDBNull(fieldSet[nameof(entity.ProgramVersionInstanceCode)]) ? null : reader.GetString(fieldSet[nameof(entity.ProgramVersionInstanceCode)]);
                            entity.ProgramID = reader.GetInt16(fieldSet[nameof(entity.ProgramID)]);
                            entity.ProgramVersionID = reader.GetInt32(fieldSet[nameof(entity.ProgramVersionID)]);
                            entity.ProgramVersionInstanceID = reader.GetInt32(fieldSet[nameof(entity.ProgramVersionInstanceID)]);
                            result.Add(entity);
                        }
                    }
                }
            }
            return _configuration = result;
        }

        private IEnumerable<string> GetHostTypeProgram(string programName, SqlConnection sqlConnection)
        {
 var commnadString = @"SELECT ht.Name
FROM [lan].[vProgram] p join
     [lan].[HostTypeProgram] tp on p.ProgramID = tp.ProgramID join
	 [lan].[HostType] ht on tp.HostTypeID = ht.HostTypeID
WHERE p.[Name] =  @Name";
            var result = new List<string>();
            using(var command = sqlConnection.CreateCommand())
            {
                command.CommandType = CommandType.Text;
                command.CommandText = commnadString;
                command.Parameters.Add("@Name", SqlDbType.VarChar, 100).Value = programName;
                using (var reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            result.Add(reader.GetString(0));
                        }
                    }
                }
            }
            return result;
        }
        private ProgramInfo GetProgramInfo(string programName, string programVesion, string hostName, string programVersionInstanceCode, SqlConnection sqlConnection)
        {
            var commandString =
            @"SELECT vc.[ProgramName]
      ,vc.[ProgramVersion]
      ,vc.[HostName]
      ,vc.[ConfigurationID]
      ,vc.[ChangeDate]
      ,vc.[ProgramVersionInstanceCode]
      ,vc.[ProgramID]
      ,vc.[ProgramVersionID]
      ,vc.[ProgramVersionInstanceID] 
	  ,c.[Name] as 'ConfigurationName'
	  ,p.[ProgramTypeID]
	  ,p.[Description] as 'ProgramDescription'
	  ,p.[ProgramTypeName]
	  ,p.[ProgramTypeDescription]
	  ,p.[ProgramFullName] 
  FROM [lan].[vConfiguration] vc with(nolock) join
       [lan].[Configuration] c on c.ConfigurationID = vc.ConfigurationID join 
       [lan].[vProgram] p on p.ProgramID = vc.ProgramID
  where vc.ProgramName = @ProgramName and
        vc.ProgramVersion = @ProgramVersion and
		vc.[HostName] = @HostName and
		vc.[ProgramVersionInstanceCode] = @ProgramVersionInstanceCode";

            using (var command = sqlConnection.CreateCommand())
            {
                command.CommandText = commandString;
                command.CommandType = CommandType.Text;
                command.Parameters.Add("@ProgramName", SqlDbType.VarChar, 100).Value = programName;
                command.Parameters.Add("@ProgramVersion", SqlDbType.VarChar, 100).Value = programVesion;
                command.Parameters.Add("@HostName", SqlDbType.VarChar, 100).Value = hostName;
                command.Parameters.Add("@ProgramVersionInstanceCode", SqlDbType.VarChar, 100).Value = programVersionInstanceCode;
                using (var reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            var result = new ProgramInfo();
                            result.ProgramName = reader.IsDBNull(reader.GetOrdinal("ProgramName")) ? null : reader.GetString(reader.GetOrdinal("ProgramName"));
                            result.ProgramVersion = reader.IsDBNull(reader.GetOrdinal("ProgramVersion")) ? null : reader.GetString(reader.GetOrdinal("ProgramVersion"));
                            result.HostName = reader.IsDBNull(reader.GetOrdinal("HostName")) ? null : reader.GetString(reader.GetOrdinal("HostName"));
                            result.ConfigurationID = reader.GetInt32(reader.GetOrdinal("ConfigurationID"));
                            result.ChangeDate = reader.GetDateTimeOffset(reader.GetOrdinal("ChangeDate"));
                            result.ProgramVersionInstanceCode = reader.IsDBNull(reader.GetOrdinal("ProgramVersionInstanceCode")) ? "" : reader.GetString(reader.GetOrdinal("ProgramVersionInstanceCode"));
                            result.ProgramID = reader.GetInt16(reader.GetOrdinal("ProgramID"));
                            result.ProgramVersionID = reader.GetInt32(reader.GetOrdinal("ProgramVersionID"));
                            result.ProgramVersionInstanceID = reader.GetInt32(reader.GetOrdinal("ProgramVersionInstanceID"));
                            result.ConfigurationName = reader.IsDBNull(reader.GetOrdinal("ConfigurationName")) ? null : reader.GetString(reader.GetOrdinal("ConfigurationName"));
                            result.ProgramTypeID = reader.GetByte(reader.GetOrdinal("ProgramTypeID"));
                            result.ProgramDescription = reader.IsDBNull(reader.GetOrdinal("ProgramDescription")) ? null : reader.GetString(reader.GetOrdinal("ProgramDescription")); 
                            result.ProgramTypeName = reader.IsDBNull(reader.GetOrdinal("ProgramTypeName")) ? null : reader.GetString(reader.GetOrdinal("ProgramTypeName"));
                            result.ProgramTypeDescription = reader.IsDBNull(reader.GetOrdinal("ProgramTypeDescription")) ? null : reader.GetString(reader.GetOrdinal("ProgramTypeDescription"));
                            result.ProgramFullName = reader.IsDBNull(reader.GetOrdinal("ProgramFullName")) ? null : reader.GetString(reader.GetOrdinal("ProgramFullName"));
                            return result;
                        }
                    }
                }
            }


            return null;
        }
        private List<ParamValues> GetParamValuesFromDataBase(string applicationName, string hostName, Version version, string programVersionInstanceCode, SqlConnection sqlConnection)
        {
var commandString =
    @"select 
         v.ConfigurationID
		,v.ConfigurationChangeDate
		,v.ParentConfigurationID
		,v.ProgramVersionID
		,v.HierarhyLevel
		,v.ParameterID
		,v.ParentParameterID
		,v.ParameterName
		,v.ParameterFullName
		,v.IntValue
		,v.DateTimeValue
		,v.StringValue
		,v.StorageTypeID
		,v.CSTypeName
		,v.ParameterGroupID
		,v.SerializationType
		,p.DefaultDateTimeValue
		,p.DefaultIntValue
		,p.DefaultStringValue
		,p.[Description]
		,p.IsReadOnly
		,p.IsRequired
		,p.IsVisible
		,st.[Name] as 'StorageTypeName'
        ,vt.[Name] as 'ValueTypeName'
	    ,vt.SQLTypeName
		,vt.DelphiTypeName
	    ,vt.CheckExpression
    from
        [lan].[ifnGetConfiguration](@ProgramName, @ProgramVersion, @HostName, @ProgramVersionInstanceCode) c
        cross apply lan.ifnGetConfugurationParamValues(c.ConfigurationID) v
        join[lan].[Parameter] p on v.ParameterID = p.ParameterID
        join[lan].[StorageType] st on v.StorageTypeID = st.StorageTypeID
        join [lan].[ValueType] vt on p.ValueTypeID = vt.ValueTypeID;";
            var result = new List<ParamValues>();
            using (var command = new SqlCommand(commandString, sqlConnection))
            {
                command.CommandType = CommandType.Text;
                command.Parameters.Add("@ProgramName", SqlDbType.VarChar, 100).Value = applicationName;
                command.Parameters.Add("@ProgramVersion", SqlDbType.VarChar, 20).Value = version.ToString();
                command.Parameters.Add("@HostName", SqlDbType.VarChar, 100).Value = hostName;
                command.Parameters.Add("@ProgramVersionInstanceCode", SqlDbType.VarChar, 100).Value = programVersionInstanceCode;

                using (var reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {

                        var configurationIdIndex = reader.GetOrdinal("ConfigurationID");
                        var configurationChangeDateIndex = reader.GetOrdinal("ConfigurationChangeDate");
                        var parentConfigurationIDIndex = reader.GetOrdinal("ParentConfigurationID");
                        var programVersionIDIndex = reader.GetOrdinal("ProgramVersionID");
                        var hierarhyLevelIndex = reader.GetOrdinal("HierarhyLevel");
                        var parameterIDIndex = reader.GetOrdinal("ParameterID");
                        var parentParameterIDIndex = reader.GetOrdinal("ParentParameterID");
                        var parameterNameIndex = reader.GetOrdinal("ParameterName");
                        var parameterFullNameIndex = reader.GetOrdinal("ParameterFullName");
                        var intValueIndex = reader.GetOrdinal("IntValue");
                        var dateTimeValueIndex = reader.GetOrdinal("DateTimeValue");
                        var stringValueIndex = reader.GetOrdinal("StringValue");
                        var storageTypeIDIndex = reader.GetOrdinal("StorageTypeID");
                        var cSTypeNameIndex = reader.GetOrdinal("CSTypeName");
                        var parameterGroupIDIndex = reader.GetOrdinal("ParameterGroupID");
                        var serializationTypeIndex = reader.GetOrdinal("SerializationType");
                        var storageTypeNameIndex = reader.GetOrdinal("StorageTypeName");
                        var descriptionIndex = reader.GetOrdinal("Description");
                        var defaultDateTimeValueIndex = reader.GetOrdinal("DefaultDateTimeValue");
                        var defaultIntValueIndex = reader.GetOrdinal("DefaultIntValue");
                        var defaultStringValueIndex = reader.GetOrdinal("DefaultStringValue");
                        var isReadOnlyIndex = reader.GetOrdinal("IsReadOnly");
                        var isRequiredIndex = reader.GetOrdinal("IsRequired");
                        var isVisibleIndex = reader.GetOrdinal("IsVisible");
                        var valueTypeNameIndex = reader.GetOrdinal("ValueTypeName");
                        var sQLTypeNameIndex = reader.GetOrdinal("SQLTypeName");
                        var checkExpressionIndex = reader.GetOrdinal("CheckExpression");
                        while (reader.Read())
                        {
                            var entity = new ParamValues
                            {
                                DateTimeValue = reader.IsDBNull(dateTimeValueIndex) ? (DateTimeOffset?)null : reader.GetDateTimeOffset(dateTimeValueIndex),
                                SerializationType = reader.IsDBNull(serializationTypeIndex) ? null : reader.GetString(serializationTypeIndex),
                                StorageTypeID = reader.GetByte(storageTypeIDIndex),
                                StringValue = reader.IsDBNull(stringValueIndex) ? null : reader.GetString(stringValueIndex),
                                CSTypeName = reader.IsDBNull(cSTypeNameIndex) ? null : reader.GetString(cSTypeNameIndex),
                                ConfigurationChangeDate = reader.IsDBNull(configurationChangeDateIndex) ? (DateTimeOffset?)null : reader.GetDateTimeOffset(configurationChangeDateIndex),
                                ConfigurationID = reader.IsDBNull(configurationIdIndex) ? (int?)null : reader.GetInt32(configurationIdIndex),
                                HierarhyLevel = reader.IsDBNull(hierarhyLevelIndex) ? (int?)null : reader.GetInt32(hierarhyLevelIndex),
                                IntValue = reader.IsDBNull(intValueIndex) ? (int?)null : reader.GetInt32(intValueIndex),
                                ParameterFullName = reader.IsDBNull(parameterFullNameIndex) ? null : reader.GetString(parameterFullNameIndex),
                                ParameterGroupID = reader.IsDBNull(parameterGroupIDIndex) ? (short?)null : reader.GetInt16(parameterGroupIDIndex),
                                ParameterID = reader.GetInt32(parameterIDIndex),
                                ParameterName = reader.IsDBNull(parameterNameIndex) ? null : reader.GetString(parameterNameIndex),
                                ParentConfigurationID = reader.IsDBNull(parentConfigurationIDIndex) ? (int?)null : reader.GetInt32(parentConfigurationIDIndex),
                                ParentParameterID = reader.IsDBNull(parentParameterIDIndex) ? (int?)null : reader.GetInt32(parentParameterIDIndex),
                                ProgramVersionID = reader.IsDBNull(programVersionIDIndex) ? (int?)null : reader.GetInt32(programVersionIDIndex),
                                StorageTypeName = reader.IsDBNull(storageTypeNameIndex) ? null : reader.GetString(storageTypeNameIndex),
                                Description = reader.IsDBNull(descriptionIndex) ? null : reader.GetString(descriptionIndex),
                                DefaultDateTimeValue = reader.IsDBNull(defaultDateTimeValueIndex) ? (DateTime?)null : reader.GetDateTime(defaultDateTimeValueIndex),
                                DefaultIntValue = reader.IsDBNull(defaultIntValueIndex) ? (int?)null : reader.GetInt32(defaultIntValueIndex),
                                DefaultStringValue = reader.IsDBNull(defaultStringValueIndex) ? null : reader.GetString(defaultStringValueIndex),
                                IsReadOnly = reader.GetBoolean(isReadOnlyIndex),
                                IsRequired = reader.GetBoolean(isRequiredIndex),
                                IsVisible = reader.GetBoolean(isVisibleIndex),
                                ValueTypeName = reader.IsDBNull(valueTypeNameIndex) ? null : reader.GetString(valueTypeNameIndex),
                                SQLTypeName = reader.IsDBNull(sQLTypeNameIndex) ? null : reader.GetString(sQLTypeNameIndex),
                                CheckExpression = reader.IsDBNull(checkExpressionIndex) ? null : reader.GetString(checkExpressionIndex),
                            };
                            result.Add(entity);
                        }
                    }
                }
            }
            return result;
        }
        public override void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);
            if (propertyName == nameof(Entities))
            {
                OnPropertyChanged(nameof(Hosts));
            }
            else if (propertyName == nameof(Hosts))
            {
                OnPropertyChanged(nameof(Host));
            }
            else if (propertyName == nameof(Host))
            {
                if (!string.IsNullOrEmpty(Host) && !Hosts.Any(o => o == Host))
                {
                    Host = null;
                }
                else
                {
                    OnPropertyChanged(nameof(ApplicationNames));
                }
            }
            else if (propertyName == nameof(ApplicationNames))
            {
                OnPropertyChanged(nameof(ApplicationName));
            }
            else if (propertyName == nameof(ApplicationName))
            {
                if (!string.IsNullOrEmpty(ApplicationName) && !ApplicationNames.Any(o => o == ApplicationName))
                {
                    ApplicationName = null;
                }
                else
                {
                    OnPropertyChanged(nameof(ProgramVersionInstanceCodes));
                }
            }
            else if (propertyName == nameof(ProgramVersionInstanceCodes))
            {
                OnPropertyChanged(nameof(ProgramVersionInstanceCode));
            }
            else if (propertyName == nameof(ProgramVersionInstanceCode))
            {
                if (!string.IsNullOrEmpty(ProgramVersionInstanceCode) && !ProgramVersionInstanceCodes.Any(o => o == ProgramVersionInstanceCode))
                {
                    ProgramVersionInstanceCode = null;
                }
            }
        }

        public bool CanExecute(object obj)
        {
            if (string.IsNullOrEmpty(ConnectionString))
                return false;
            if (string.IsNullOrEmpty(Host))
                return false;
            if (string.IsNullOrEmpty(ApplicationName))
                return false;
            if (string.IsNullOrEmpty(ProgramVersionInstanceCode))
            {
                if (!ProgramVersionInstanceCodes.Any(o => string.IsNullOrEmpty(o)))
                    return false;
            }
            return true;
        }
        public void Initialize()
        {
            if (!string.IsNullOrEmpty(ConnectionString))
                return;
            try
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
                var connectionString = parameters.Where(o => o.Attribute("Name")?.Value == "ConnectionString").FirstOrDefault();
                var host = parameters.Where(o => o.Attribute("Name")?.Value == "Host").FirstOrDefault();
                var applicationName = parameters.Where(o => o.Attribute("Name")?.Value == "ApplicationName").FirstOrDefault();
                var programVersionInstanceCode = parameters.Where(o => o.Attribute("Name")?.Value == "ProgramVersionInstanceCode").FirstOrDefault();
                if (isCurrent != null && int.TryParse(isCurrent.Value, out int isCurrentValue))
                {
                    IsCurrent = isCurrentValue == 1;
                }
                if (syncRequired != null && int.TryParse(syncRequired.Value, out int syncRequiredValue))
                {
                    SyncRequired = syncRequiredValue == 1;
                }
                if (connectionString != null)
                {
                    ConnectionString = connectionString.Value;
                }
                if (host != null)
                {
                    Host = host.Value;
                }
                if (applicationName != null)
                {
                    ApplicationName = applicationName.Value;
                }
                if (programVersionInstanceCode != null)
                {
                    ProgramVersionInstanceCode = programVersionInstanceCode.Value;
                }
                if (RefreshCommandCanExecute(null))
                {
                    RefreshCommand.Execute(true);
                    if (host != null)
                    {
                        Host = host.Value;
                    }
                    if (applicationName != null)
                    {
                        ApplicationName = applicationName.Value;
                    }
                    if (programVersionInstanceCode != null)
                    {
                        ProgramVersionInstanceCode = programVersionInstanceCode.Value;
                    }
                }
                    
            }
            catch (Exception ex)
            {
                throw new ConfiguratorException($"Во время инициализации правметров для {Name} произошла ошибка {ex.ToLogString()}");
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
            var connectionString = parameters.Where(o => o.Attribute("Name")?.Value == "ConnectionString").FirstOrDefault();
            var host = parameters.Where(o => o.Attribute("Name")?.Value == "Host").FirstOrDefault();
            var applicationName = parameters.Where(o => o.Attribute("Name")?.Value == "ApplicationName").FirstOrDefault();
            var programVersionInstanceCode = parameters.Where(o => o.Attribute("Name")?.Value == "ProgramVersionInstanceCode").FirstOrDefault();

            if (isCurrent != null)
            {
                isCurrent.Value = IsCurrent == true ? "1" : "0";
            }
            if (syncRequired != null)
            {
                syncRequired.Value = SyncRequired == true ? "1" : "0";
            }
            if (connectionString != null)
            {
                connectionString.Value = ConnectionString;
            }
            if (host != null)
            {
                host.Value = Host;
            }
            if (applicationName != null)
            {
                applicationName.Value = ApplicationName;
            }
            if (programVersionInstanceCode != null)
            {
                programVersionInstanceCode.Value = ProgramVersionInstanceCode ?? "";
            }
            if (saveDocument)
            {
                var configPath = Path.Combine(Path.GetTempPath(), Assembly.GetExecutingAssembly().GetName().Name);
                document.Save(configPath);
            }          
        }

        public ISynchronizationManager Clone()
        {
            var result = new TopologyDbSynchronizationManager(ConfigurationProvider, false);
            result.Entities = Entities;
            result.Name = Name;
            result.IsCurrent = IsCurrent;
            result.SyncRequired = false;          
            result.ConnectionString = ConnectionString;
            result.Host = Host;
            result.ApplicationName = ApplicationName;
            result.ProgramVersionInstanceCode = ProgramVersionInstanceCode;
            return result;
        }
    }
    public partial class VConfiguration
    {
        public string ProgramName { get; set; }
        public string ProgramVersion { get; set; }
        public string HostName { get; set; }
        public int ConfigurationID { get; set; }
        public DateTimeOffset ChangeDate { get; set; }
        public string ProgramVersionInstanceCode { get; set; }
        public short ProgramID { get; set; }
        public int ProgramVersionID { get; set; }
        public int ProgramVersionInstanceID { get; set; }
    }
    public class ProgramInfo
    {
        public string ProgramName { get; set; }
        public string ProgramVersion { get; set; }
        public string HostName { get; set; }
        public int ConfigurationID { get; set; }
        public DateTimeOffset ChangeDate { get; set; }
        public string ProgramVersionInstanceCode { get; set; }
        public short ProgramID { get; set; }
        public int ProgramVersionID { get; set; }
        public int ProgramVersionInstanceID { get; set; }
        public string ConfigurationName { get; set; }
        public byte ProgramTypeID { get; set; }
        public string ProgramDescription { get; set; }
        public string ProgramTypeName { get; set; }
        public string ProgramTypeDescription { get; set; }
        public string ProgramFullName { get; set; }
    }

    public class ParamValues
    {
        public int? ConfigurationID { get; set; }
        public DateTimeOffset? ConfigurationChangeDate { get; set; }
        public int? ParentConfigurationID { get; set; }
        public int? ProgramVersionID { get; set; }
        public int? HierarhyLevel { get; set; }
        public int ParameterID { get; set; }
        public int? ParentParameterID { get; set; }
        public string ParameterName { get; set; }
        public string ParameterFullName { get; set; }
        public int? IntValue { get; set; }
        public DateTimeOffset? DateTimeValue { get; set; }
        public string StringValue { get; set; }
        public byte StorageTypeID { get; set; }
        public string CSTypeName { get; set; }
        public short? ParameterGroupID { get; set; }
        public string SerializationType { get; set; }
        public DateTime? DefaultDateTimeValue { get; set; }
        public int? DefaultIntValue { get; set; }
        public string DefaultStringValue { get; set; }
        public string Description { get; set; }
        public bool IsReadOnly { get; set; }
        public bool IsRequired { get; set; }
        public bool IsVisible { get; set; }
        public string StorageTypeName { get; set; }
        public string ValueTypeName { get; set; }
        public string SQLTypeName { get; set; }
        public string CheckExpression { get; set; }
    }
}
