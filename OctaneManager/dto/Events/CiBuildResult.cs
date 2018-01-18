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

namespace MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Dto.Events
{
	public class CiBuildResult : IDtoBase
	{
		private readonly string _value;
		private CiBuildResult(string type)
		{
			_value = type;
		}

		public static CiBuildResult Unavailable => new CiBuildResult("unavailable");
		public static CiBuildResult Unstable => new CiBuildResult("unstable");
		public static CiBuildResult Aborted => new CiBuildResult("aborted");
		public static CiBuildResult Failure => new CiBuildResult("failure");
		public static CiBuildResult Success => new CiBuildResult("success");

		public override string ToString()
		{
			return _value;
		}
	}
}
