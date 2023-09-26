using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace mop.Configurator
{
    public class ProgramsViewModel: BaseViewModel
    {
        const string ATTR_CONFIGURATION_ID = "ConfigurationID";
        const string ATTR_PROGRAM_TYPE_ID = "ProgramTypeID";
        const string ATTR_PROGRAM_TYPE_NAME = "ProgramTypeName";
        public ProgramsViewModel()
        {
           
      
        }


        private string _connectionString = @"Server=.\SQL2019SRV;Database=TopologyDB1;Trusted_Connection=True; User ID=sa; Password=Qwerty11;Async=True";

        public string ConnectionString
        {
            get { return _connectionString; }
            set
            {
                if(_connectionString != value)
                {
                    _connectionString = value;
                    OnPropertyChanged(nameof(ConnectionString));
                }
            }
        }

        //private async Task<IEnumerable<Parameter>> GetParametersAsync(CancellationToken cancellationToken = default(CancellationToken))
        //{
        //    var entities = await GetLastHeartBeatsAsync(cancellationToken);
        //    if (entities.Count() == 0)
        //        return Enumerable.Empty<Parameter>();
        //    var hosts = entities.Select(o=> )
        //}

        //private Parameter CreateHostParameter(string hostName)
        //{
        //    var result = new Parameter();
        //    result.Name
        //}
        private async Task<IEnumerable<LastHeartBeat>> GetLastHeartBeatsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (string.IsNullOrEmpty(_connectionString))
            {
                throw new ArgumentNullException(nameof(ConnectionString), "В качестве строки подключения передано пустое значение");
            }
            var getLastHeartBeatsCommandText = @"
select c.[ProgramName]
      ,c.[ProgramVersion]
      ,c.[HostName]
      ,c.[ConfigurationID]
      ,c.[ChangeDate]
      ,c.[ProgramVersionInstanceCode]
      ,c.[ProgramID]
      ,c.[ProgramVersionID]
      ,c.[ProgramVersionInstanceID]
	  ,h.[ConfigChangeNotificationTime]
	  ,h.[StateID]
	  ,h.[StateName]
	  ,p.[ProgramTypeID]
	  ,pt.[Name] as ProgramTypeName
 from [lan].[vConfiguration] c cross apply
       (
	      select top(1) [PVIHeartbeatID]
				,[ProgramVersionInstanceID]
				,[StateID]
				,[CreateTime]
				,[ConfigChangeNotificationTime]
				,s.[Name] as StateName
          from [lan].[PVIHeartbeat] hb join 
		       [lan].[PVIState] s on hb.[StateID] = s.[PVIStateID]
		  where hb.[ProgramVersionInstanceID] = c.[ProgramVersionInstanceID]
		  order by [CreateTime] desc
	   ) h    join
      [lan].[Program] p on c.[ProgramID] = p.[ProgramID] join
      [lan].[ProgramType] pt on p.[ProgramTypeID] = pt.[ProgramTypeID]
       ";

            var result = new List<LastHeartBeat>();
            using(var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using(var command = connection.CreateCommand())
                {
                    command.CommandType = CommandType.Text;
                    command.CommandText = getLastHeartBeatsCommandText;
                    using(var reader =  command.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            var programNameIndex = reader.GetOrdinal("ProgramName");
                            var programVersionIndex = reader.GetOrdinal("ProgramVersion");
                            var hostNameIndex = reader.GetOrdinal("HostName");
                            var configurationIDIndex = reader.GetOrdinal("ConfigurationID");
                            var changeDateIndex = reader.GetOrdinal("ChangeDate");
                            var programVersionInstanceCodeIndex = reader.GetOrdinal("ProgramVersionInstanceCode");
                            var programIDIndex = reader.GetOrdinal("ProgramID");
                            var programVersionIDIndex = reader.GetOrdinal("ProgramVersionID");
                            var programVersionInstanceIDIndex = reader.GetOrdinal("ProgramVersionInstanceID");
                            var configChangeNotificationTimeIndex = reader.GetOrdinal("ConfigChangeNotificationTime");
                            var stateIDIndex = reader.GetOrdinal("StateID");
                            var stateNameIndex = reader.GetOrdinal("StateName");
                            var programTypeIDIndex = reader.GetOrdinal("ProgramTypeID");
                            var programTypeNameIndex = reader.GetOrdinal("ProgramTypeName");

                            while (await reader.ReadAsync(cancellationToken))
                            {
                                cancellationToken.ThrowIfCancellationRequested();
                                var entity = new LastHeartBeat();
                                entity.ProgramName = reader.GetString(programNameIndex);
                                entity.ProgramVersion = reader.GetString(programVersionIndex);
                                entity.HostName = reader.GetString(hostNameIndex);
                                entity.ConfigurationID = reader.GetInt32(configurationIDIndex);
                                entity.ChangeDate = reader.GetDateTimeOffset(changeDateIndex);
                                entity.ProgramVersionInstanceCode = reader.GetString(programVersionInstanceCodeIndex);
                                entity.ProgramID = reader.GetInt16(programIDIndex);
                                entity.ProgramVersionID = reader.GetInt16(programVersionIDIndex);
                                entity.ProgramVersionInstanceID = reader.GetInt16(programVersionInstanceIDIndex);
                                entity.ConfigChangeNotificationTime = reader.IsDBNull(configChangeNotificationTimeIndex) ? (DateTimeOffset?)null : reader.GetDateTimeOffset(configChangeNotificationTimeIndex);
                                entity.StateID = reader.GetByte(stateIDIndex);
                                entity.StateName = reader.GetString(stateNameIndex);
                                entity.ProgramTypeID = reader.GetByte(programTypeIDIndex);
                                entity.ProgramTypeName = reader.GetString(programTypeNameIndex);
                                result.Add(entity);
                            }
                        }
                    }
                }
            }
            return result;
        }
        public class LastHeartBeat
        {
            public string ProgramName { get; set; }
            public string ProgramVersion { get; set; }
            public string HostName { get; set; }
            public int ConfigurationID { get; set; }
            public DateTimeOffset ChangeDate { get; set; }
            public string ProgramVersionInstanceCode { get; set; }
            public short ProgramID { get; set; }
            public short ProgramVersionID { get; set; }
            public short ProgramVersionInstanceID { get; set; }
            public DateTimeOffset? ConfigChangeNotificationTime { get; set; }
            public byte StateID { get; set; }
            public string StateName { get; set; }
            public byte ProgramTypeID { get; set; }
            public string ProgramTypeName { get; set; }
        }
    }
}
