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
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Dto.General;
using System;
using System.IO;
using System.Text;

namespace MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Octane
{
	public static class OctaneUtils
	{
		public static long ConvertToOctaneTime(DateTime data)
		{
			var span = data.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, data.Kind));
			return (long)span.TotalMilliseconds;
		}

		public static string GenerateOctaneJobCiId(string collectionName, string projectId, string buildDefId)
		{
			var id = $"{collectionName.Replace(" ", "-")}.{projectId}.{buildDefId}".ToLower();
			return id;
		}

		public static TfsCiEntity TranslateOctaneJobCiIdToObject(string id)
		{
			var parts = id.Split('.');
			var tfsCiEntity = new TfsCiEntity(parts[0], parts[1], parts[2]);

			return tfsCiEntity;
		}
	}

	public class Utf8StringWriter : StringWriter
	{
		public override Encoding Encoding => Encoding.UTF8;
	}
}
