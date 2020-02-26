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
using Newtonsoft.Json;
using System;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Configuration.Credentials;
using System.Net;

namespace MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Configuration
{
    public class ProxyDetails : ICloneable
    {
        public static string SENSITIVE_VALUE_REPLACER = "**********";

        public bool Enabled { get; set; } = true;

        public string Host { get; set; }

        public String Port { get; set; }

        public string User { get; set; }

        public string Password { get; set; }

        public string BypassList { get; set; }

        public bool BypassLocal { get; set; } = true;

        public ProxyDetails()
        {

        }

        public ProxyDetails(bool enabled, string host, String port, string user, string password, bool bypassLocal, string bypassList)
        {
            Enabled = enabled;
            Host = host;
            Port = port;
            User = user;
            Password = password;
            BypassList = bypassList;
            BypassLocal = bypassLocal;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public void Encrypt()
        {
            if (String.IsNullOrEmpty(Password))
            {
                Password = "";
            }
            else
            {
                Password = Encryption.Encrypt(Password);
            }
            
        }


        public void Decrypt()
        {
            Password = Encryption.Decrypt(Password);
        }

        public void ResetSensitiveInfoTo(ProxyDetails other)
        {
            if (SENSITIVE_VALUE_REPLACER.Equals(other.Password))
            {
                other.Password = this.Password;
            }
        }

        public ProxyDetails GetInstanceWithoutSensitiveInfo()
        {
            var clone = Clone() as ProxyDetails;
            clone.Password = SENSITIVE_VALUE_REPLACER;
            return clone;
        }

        public ProxyDetails GetSecuredInstance()
        {
            var clone = Clone() as ProxyDetails;
            clone.Encrypt();
            return clone;
        }

        public WebProxy ToWebProxy()
        {
            if (!Enabled || String.IsNullOrEmpty(Host))
            {
                return null;
            }
            int port = Int32.Parse(Port);
            WebProxy wp = new WebProxy(Host, port);
            wp.BypassProxyOnLocal = BypassLocal;
            if (String.IsNullOrEmpty(BypassList))
            {
                wp.BypassList = BypassList.Split(';');
            }
            if (!String.IsNullOrEmpty(User))
            {
                wp.Credentials = new NetworkCredential(User, Password);
            }


            return wp;

        }
    }
}
