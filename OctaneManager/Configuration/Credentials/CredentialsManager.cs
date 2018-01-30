/*!
* (c) 2016-2018 EntIT Software LLC, a Micro Focus company
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/
/*
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
*/