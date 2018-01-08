using System;

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
