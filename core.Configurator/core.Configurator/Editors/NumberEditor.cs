using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mop.Configurator.Editors
{
    public class NumberEditor : EditorBase
    {        
        public override string[] SuitableNames { get; set; } = new[] { "int", "long" };
        public override int Sequence { get; set; } = 1;
        public override string Header { get; set; } = "Число";
        public override string this[string columnName]
        {
            get
            {
                var error = base[columnName];
                if (string.IsNullOrEmpty(error))
                {
                    if (!decimal.TryParse(Value, out decimal value))
                        error = "Значение должно быть числом.";
                }
                return Error = error;
            }
        }
    }
}
