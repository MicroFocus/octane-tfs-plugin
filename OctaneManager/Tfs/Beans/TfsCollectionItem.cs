using System;

namespace MicroFocus.Ci.Tfs.Octane.Tfs.Beans
{
    public class TfsCollectionItem :TfsItemBase
    {
        public TfsCollectionItem(Guid id, string name) : base(id.ToString(),name)
        {

        }


    }
}
