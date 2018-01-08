

namespace MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Dto.Events
{
    internal class PhaseType
    {
        private readonly string _value;
        private PhaseType(string type)
        {
            _value = type;
        }

        public static PhaseType Post => new PhaseType("post");
        public static PhaseType Internal => new PhaseType("internal");       

        public override string ToString()
        {
            return _value;
        }
    }
}
