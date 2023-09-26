using System;
using System.Globalization;

namespace mop.Configurator.Editors
{
    public class TimeSpanEditor: EditorBase
    {
        public override string[] SuitableNames { get; set; } = new[] { "timeSpan" };
        public override int Sequence { get; set; } = 5;
        public override string Header { get; set; } = "Отрезок времени";
        public override string this[string columnName]
        {
            get
            {
                var error = base[columnName];
                if (string.IsNullOrEmpty(error))
                {
                    if (!DateTime.TryParseExact(Value.ToString(), "HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateResult))
                        error = "Ожидается значение в формате времени (Часы:Минуты:Секунды).";
                }
                return Error = error;
            }
        }
    }
}
