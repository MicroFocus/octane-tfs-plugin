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
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tools.Connectivity;

namespace MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Configuration
{
    public class ConnectionDetails : ICloneable
    {
        public static string SENSITIVE_VALUE_REPLACER = "**********";

        [JsonIgnore]
        public string Host
        {
            get
            {
                Uri uri = new Uri(ALMOctaneUrl);
                string host = $"{uri.Scheme}://{uri.Host}:{uri.Port}";
                return host;
            }
        }

        public String TfsVersion
        {
            get
            {
                return RunModeManager.GetInstance().TfsVersion.ToString();
            }
        }

        [JsonIgnore]
        public int SharedSpace
        {
            get
            {
                try
                {
                    var index = ALMOctaneUrl.IndexOf("=", StringComparison.Ordinal);
                    var sharedSpaceStr = ALMOctaneUrl.Substring(index + 1);
                    var endIndex = sharedSpaceStr.IndexOf("/", StringComparison.Ordinal);
                    if (endIndex > -1)
                    {
                        sharedSpaceStr = sharedSpaceStr.Substring(0, endIndex);
                    }
                    var res = int.Parse(sharedSpaceStr);
                    return res;
                }
                catch (Exception)
                {
                    throw new Exception($"Sharedspace id is expected, but not received in {ALMOctaneUrl}");
                }
            }
        }
        public string ALMOctaneUrl { get; set; }

        public string ClientId { get; set; }

        public string ClientSecret
        {
            get;
            set;
        }

        /// <summary>
        /// For tfs 2017
        /// </summary>
		public string Pat
        {
            get;
            set;
        }

        /// <summary>
        /// For tfs 2015
        /// </summary>
        public string TfsUser { get; set; } = "";

        /// <summary>
        /// For tfs 2015
        /// </summary>
        public string TfsPassword { get; set; } = "";

        public string TfsLocation { get; set; }

        public ConnectionDetails()
        {

        }

        public ConnectionDetails(string octaneUrl, string clientId, string clientSecret, string tfsLocation, string instanceId)
        {
            ALMOctaneUrl = octaneUrl;
            ClientId = clientId;
            ClientSecret = clientSecret;
            InstanceId = (instanceId == null ? Guid.NewGuid().ToString() : instanceId);
            TfsLocation = tfsLocation;
        }

        public string InstanceId { get; set; } = Guid.NewGuid().ToString();



        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public void Encrypt()
        {
            Pat = Encryption.Encrypt(Pat);
            ClientSecret = Encryption.Encrypt(ClientSecret);
            if (!string.IsNullOrEmpty(TfsPassword))
                TfsPassword = Encryption.Encrypt(TfsPassword);
        }


        public void Decrypt()
        {
            Pat = Encryption.Decrypt(Pat);
            ClientSecret = Encryption.Decrypt(ClientSecret);
            if (!string.IsNullOrEmpty(TfsPassword))
                TfsPassword = Encryption.Decrypt(TfsPassword);
        }

        public void ResetSensitiveInfoTo(ConnectionDetails other)
        {
            if (SENSITIVE_VALUE_REPLACER.Equals(other.Pat))
            {
                other.Pat = this.Pat;
            }

            if (SENSITIVE_VALUE_REPLACER.Equals(other.TfsPassword))
            {
                other.TfsPassword = this.TfsPassword;
            }


            if (SENSITIVE_VALUE_REPLACER.Equals(other.ClientSecret))
            {
                other.ClientSecret = this.ClientSecret;
            }
        }

        public ConnectionDetails GetInstanceWithoutSensitiveInfo()
        {
            var clone = Clone() as ConnectionDetails;
            clone.ClientSecret = SENSITIVE_VALUE_REPLACER;
            clone.Pat = SENSITIVE_VALUE_REPLACER;
            clone.TfsPassword = SENSITIVE_VALUE_REPLACER;
            return clone;
        }

        public ConnectionDetails GetSecuredInstance()
        {
            var clone = Clone() as ConnectionDetails;
            clone.Encrypt();
            return clone;
        }
    }
}
