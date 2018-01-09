
namespace MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Dto.Events
{
	public class CiEventCauseType : IDtoBase
    {
        private string _value;
        private CiEventCauseType(string type)
        {
            _value = type;
        }

        public static CiEventCauseType Scm => new CiEventCauseType("scm");
        public static CiEventCauseType User => new CiEventCauseType("user");
        public static CiEventCauseType Timer => new CiEventCauseType("timer");
        public static CiEventCauseType Upstream => new CiEventCauseType("upstream");
        public static CiEventCauseType Undefined => new CiEventCauseType("undefined");

        public override string ToString()
        {
            return _value;
        }
    }
}
