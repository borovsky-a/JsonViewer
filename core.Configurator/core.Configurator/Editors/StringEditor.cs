namespace mop.Configurator.Editors
{
    public class StringEditor : EditorBase
    {       
        public override string[] SuitableNames { get; set; } = new[] { "string" };
        public override int Sequence { get; set; } = 0;
        public override string Header { get; set; } = "Строка";
        public override bool IsDefault { get; set; } = true;
       
    }
}
