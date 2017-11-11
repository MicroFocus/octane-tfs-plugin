using System;

namespace MicroFocus.Ci.Tfs.Octane.Tfs.Beans
{
    public abstract class TfsItemBase
    {
        public string Id { get; protected set; }
        public string Name { get; protected set; }


        protected TfsItemBase(string id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
