using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CredentialManagement;

namespace MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Configuration.Credentials
{
    public  class CredentialsManager
    {

        public const string AlmOctaneCredentials = "ALMOctaneCredentials";
        public const string TfsOctaneCredentials = "TFSPat";

        public static void SaveSecret(string type,string secret)
        {
            using (var cred = new Credential())
            {
                cred.Password = secret;
                cred.Target = type;
                cred.Type = CredentialType.Generic;
                cred.PersistanceType = PersistanceType.LocalComputer;
                cred.Save();
            }
        }

        public static string GetSecret(string type)
        {
            using (var cred = new Credential())
            {
                cred.Target = type;
                cred.Load();
                return cred.Password;
            }
        }
    }
}
