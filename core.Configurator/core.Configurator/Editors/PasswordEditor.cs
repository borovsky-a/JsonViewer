using System.Linq;

namespace mop.Configurator.Editors
{
    public class PasswordEditor : EditorBase
    {       
        public override string[] SuitableNames { get; set; } = new[] { "password" };
        public override int Sequence { get; set; } = 4;
        public override string Header { get; set; } = "Пароль";
        public override bool IsDefault { get; set; } = false;  
        public override bool IsVisible
        {
            get { return base.IsVisible && SuitableNames.Any(o => o.Equals(Parameter.EditorType, System.StringComparison.InvariantCultureIgnoreCase)); }
            set
            {
                base.IsVisible = value;
            }
        }
    }
}
