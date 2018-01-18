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
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tools;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Dto.Connectivity
{
	public abstract class OctaneTaskBase : IDtoBase
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("headers")]
        public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();

        [JsonProperty("serviceId")]
        public Guid? ServiceId { get; set; } = null;

        [JsonProperty("body")]
        public string Body { get; set; }

        public override string ToString()
        {
            return JsonHelper.SerializeObject(this);
        }
    }
}
