
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using mop.Configurator.Editors.DataConnection;

namespace mop.Configurator.Editors.DataConnection
{
    public class DataConnectionDialogConfiguration
    {
        public static DataConnectionDialogConfiguration Default { get; }

        static DataConnectionDialogConfiguration()
        {
            Default = new DataConnectionDialogConfiguration();
            Default.DataSources.Add(DataSource.SqlDataSource);
            Default.DataSources.Add(DataSource.SqlFileDataSource);
            Default.DataSources.Add(DataSource.OracleDataSource);
            Default.DataSources.Add(DataSource.AccessDataSource);
            Default.DataSources.Add(DataSource.OdbcDataSource);
      
            Default.UnspecifiedDataSource.Providers.Add(DataProvider.SqlDataProvider);
            Default.UnspecifiedDataSource.Providers.Add(DataProvider.OracleDataProvider);
            Default.UnspecifiedDataSource.Providers.Add(DataProvider.OleDBDataProvider);
            Default.UnspecifiedDataSource.Providers.Add(DataProvider.OdbcDataProvider);
            Default.DataSources.Add(Default.UnspecifiedDataSource);

            Default.SelectedDataProvider = DataProvider.SqlDataProvider;
            Default.SelectedDataSource = DataSource.SqlDataSource;
        }

        public DataConnectionDialogConfiguration()
        {
            DataSources = new List<DataSource>();
            UnspecifiedDataSource= DataSource.CreateUnspecified();

            SelectedDataProvider = DataProvider.SqlDataProvider;
            SelectedDataSource = DataSource.SqlDataSource;
        }

        public ICollection<DataSource> DataSources { get; set; }
        public DataSource UnspecifiedDataSource { get; set; }
        public DataSource SelectedDataSource { get; set; }
        public DataProvider SelectedDataProvider { get; set; }

        internal void ApplyConfiguration(DataConnectionDialog dialog)
        {
            foreach (var ds in DataSources)
            {
                dialog.DataSources.Add(ds);               
            }
            foreach (var provider in UnspecifiedDataSource.Providers)
            {
                dialog.UnspecifiedDataSource.Providers.Add(provider);
            }
            dialog.DataSources.Add(UnspecifiedDataSource);
            dialog.SelectedDataSource = SelectedDataSource;
           // dialog.SelectedDataProvider = SelectedDataProvider;

        }
    }
}
