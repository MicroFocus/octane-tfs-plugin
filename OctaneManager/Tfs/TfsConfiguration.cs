using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tfs
{
    public class TfsConfiguration
    {

        public Uri Uri{get; protected set; }
        public string Pat { get; protected set; }

        public TfsConfiguration(Uri uri, string pat)
        {
            Uri = uri;
            Pat = pat;
        }
    }
}
