using mop.Configurator.Editors.DataConnection;
using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Windows.Input;

namespace mop.Configurator.Editors
{
    public class SqlConnectionEditor : EditorBase
    {
        public override string[] SuitableNames { get; set; } = new string[] { "connection" };
        public override int Sequence { get; set; } = 3;
        public override string Header { get; set; } = "Строка подключения";

        public override string this[string columnName]
        {
            get
            {
                var error = base[columnName];
                if (string.IsNullOrEmpty(error))
                {
                    try
                    {
                        DbConnectionStringBuilder csb = new DbConnectionStringBuilder();
                        csb.ConnectionString = Value;
                        csb = null;
                    }
                    catch
                    {
                        error = "Ожидается строка подключения.";
                    }
                }
                return Error = error;
            }
        }

        public ICommand EditCommand => new RelayCommand(EditCommandExecute, o => true);

        private void EditCommandExecute(object parameter)
        {
            try
            {
                string connectionString = null;
                try
                {
                    connectionString = new SqlConnectionStringBuilder(Value) { IntegratedSecurity = true }.ConnectionString;
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
                    if (DataConnectionDialog.Show(dialog) == DialogResult.OK)
                    {
                        Value = dialog.ConnectionString;
                    }
                }
            }
            catch (Exception ex)
            {
                Error = ex.ToString();
            }
        }
    }
}
