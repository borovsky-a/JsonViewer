using mop.Configurator.Editors.Account;
using System;
using System.Collections;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;

namespace mop.Configurator.Editors
{
    public class AccountEditor : EditorBase
    {
        private string _accountValueType = "0";
        private string _innerException;
        public AccountEditor()
        {
            UseDefaultUserSettings();
        }
        /// <summary>
        ///     Настройки для инициализации параметра
        /// </summary>
        public ADObjectSettings ParameterSettings { get; set; } = new ADObjectSettings();
        public override string[] SuitableNames { get; set; } = new string[] { "account" };
        public override int Sequence { get; set; } = 2;
        public override string Header { get; set; } = "Аккаунт";

        public bool IsCancel { get; private set; }
        public override string this[string columnName]
        {
            get
            {
                var error = base[columnName] ?? _innerException;               
                return Error = error;
            }
        }

        public ICommand EditCommand => new RelayCommand(EditCommandExecute, o => true);
        public ICommand AccountValueTypeCommand => new RelayCommand(AccountValueTypeCommandExecute, o => true);

        /// <summary>
        ///     Реализует шаблон Builder. Применяет значения по умолчанию для выбора пользователя
        /// </summary>
        public AccountEditor UseDefaultUserSettings()
        {
            ParameterSettings.DefaultLocations = Locations.All;
            ParameterSettings.AllowedLocations = Locations.All;
            ParameterSettings.DefaultTypes = ObjectTypes.Users | ObjectTypes.WellKnownPrincipals | ObjectTypes.ServiceAccounts;
            ParameterSettings.AllowedTypes = ObjectTypes.Users | ObjectTypes.WellKnownPrincipals | ObjectTypes.ServiceAccounts | ObjectTypes.Computers;
            return this;
        }

        /// <summary>
        ///     Реализует шаблон Builder. Применяет значения по умолчанию для выбора групп
        /// </summary>
        public AccountEditor UseDefaultGroupSettings()
        {
            ParameterSettings.DefaultLocations = Locations.All;
            ParameterSettings.AllowedLocations = Locations.All;
            ParameterSettings.DefaultTypes = ObjectTypes.Groups | ObjectTypes.Computers | ObjectTypes.BuiltInGroups;
            ParameterSettings.AllowedTypes = ObjectTypes.Groups | ObjectTypes.BuiltInGroups | ObjectTypes.Computers; ;
            return this;
        }
        private void EditCommandExecute(object parameter)
        {
            try
            {
                using (var form = new Form() { StartPosition = FormStartPosition.CenterScreen })
                using (var picker = new DirectoryObjectPickerDialog())
                {
                    var helper = new WindowInteropHelper(System.Windows.Application.Current.MainWindow);
                    SetWindowLong(new HandleRef(form, form.Handle), -8, helper.Handle.ToInt32());

                    picker.AllowedObjectTypes = ParameterSettings.AllowedTypes;
                    picker.DefaultObjectTypes = ParameterSettings.DefaultTypes;
                    picker.AllowedLocations = ParameterSettings.AllowedLocations;
                    picker.DefaultLocations = ParameterSettings.DefaultLocations;
                    picker.MultiSelect = false;
                    picker.SkipDomainControllerCheck = false;
                    picker.ShowAdvancedView = true;
                    picker.AttributesToFetch.Add("objectSid");

                    DialogResult dialogResult = picker.ShowDialog(form);

                    if (dialogResult == DialogResult.OK)
                    {
                        _innerException = null;
                        IsCancel = false;
                        DirectoryObject result = picker.SelectedObjects.OfType<DirectoryObject>().FirstOrDefault();
                        if (result == null)
                            return;

                        var value = new ADObjectResult();


                        for (int j = 0; j < result.FetchedAttributes.Length; j++)
                        {

                            object multivaluedAttribute = result.FetchedAttributes[j];
                            if (!(multivaluedAttribute is IEnumerable) || multivaluedAttribute is byte[] || multivaluedAttribute is string)
                                multivaluedAttribute = new[] { multivaluedAttribute };

                            foreach (object attribute in (IEnumerable)multivaluedAttribute)
                            {
                                if (attribute != null && attribute is byte[])
                                {
                                    byte[] bytes = (byte[])attribute;
                                    value.Sid = BytesToString(bytes);

                                    if(_accountValueType == "0")
                                    {
                                        var sid = new SecurityIdentifier(value.Sid);
                                        var acct = (NTAccount)sid.Translate(typeof(NTAccount));
                                        var name = acct.ToString();
                                        if (string.IsNullOrEmpty(name))
                                        {
                                            _innerException = "Не удалось получить имя пользователя";
                                            value.Name = _innerException;
                                        }
                                        else
                                        {
                                            value.Name = name;
                                        }    
                                    }   
                                }
                            }
                        }
                        if (_accountValueType == "0")
                            Value = value.Name;
                        else if (_accountValueType == "1")
                            Value = value.Sid;
                    }
                    else IsCancel = true;
                }
            }
            catch (Exception ex)
            {
                _innerException = ex.ToString();
                OnPropertyChanged(nameof(Value));
            }
        }
        private string BytesToString(byte[] bytes)
        {
            try
            {
                Guid guid = new Guid(bytes);
                return guid.ToString("D");
            }
            catch (Exception)
            {
                //ignored
            }
            try
            {
                SecurityIdentifier sid = new SecurityIdentifier(bytes, 0);
                return sid.ToString();
            }
            catch (Exception)
            {
                //ignored
            }
            return "0x" + BitConverter.ToString(bytes).Replace('-', ' ');
        }
        private void AccountValueTypeCommandExecute(object parameter)
        {
            _accountValueType = parameter.ToString();
        }
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(HandleRef hWnd, int nIndex, int dwNewLong);     
    }
}
