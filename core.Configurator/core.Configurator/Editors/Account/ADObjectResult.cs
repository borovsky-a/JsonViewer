using System.Text;

namespace mop.Configurator.Editors.Account
{
    /// <summary>
    ///     Результат, возвращяемый диалоговым окном выбора пользователей
    /// </summary>
    public class ADObjectResult
    {       

        /// <summary>
        ///     NTAccountName
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     SID
        /// </summary>
        public string Sid { get; set; }
       
    }
}
