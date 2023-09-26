

namespace mop.Configurator.Editors.Account
{
    /// <summary>
    ///     Параметры иннициализации <see cref="ADObjectParameter"/>
    /// </summary>
    public class ADObjectSettings
    {
        /// <summary>
        ///     Название параметра имени аккаунта в конфигурации
        /// </summary>
        public string NamePropetyMapName { get; set; }

        /// <summary>
        ///     Название параметра SID в конфигурации
        /// </summary>
        public string SidPropetyMapName { get; set; }

        /// <summary>
        ///     Разрешенные типы для поиска аккаунтов
        /// </summary>
        public ObjectTypes AllowedTypes { get; set; }

        /// <summary>
        ///     Типы по умолчанию для поиска аккаунтов
        /// </summary>
        public ObjectTypes DefaultTypes { get; set; }

        /// <summary>
        ///     Разрешенные директории для поиска аккаунтов
        /// </summary>
        public Locations AllowedLocations { get; set; }

        /// <summary>
        ///     Директории по умолчанию для поиска аккаунтов
        /// </summary>
        public Locations DefaultLocations { get; set; }
    }
}
