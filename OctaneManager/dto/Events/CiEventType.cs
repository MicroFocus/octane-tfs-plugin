
using MicroFocus.Ci.Tfs.Octane.dto;

namespace MicroFocus.Ci.Tfs.Octane.Dto.Events
{
    public class CiEventType : IDtoBase
    {
        private string _value;
        public CiEventType(string type)
        {
            _value = type;
        }

        public static CiEventType Undefined => new CiEventType("undefined");
        public static CiEventType Queued => new CiEventType("queued");
        public static CiEventType Started => new CiEventType("started");
        public static CiEventType Finished => new CiEventType("finished");
        public static CiEventType Scm => new CiEventType("scm");

		public override bool Equals(object obj)
		{
			if (obj == null || GetType() != obj.GetType())
			{
				return false;
			}

			return _value.Equals(((CiEventType)obj)._value);
		}

		public override int GetHashCode()
		{
			return _value.GetHashCode();
		}

		public override string ToString()
        {
            return _value;
        }        
    }
}
