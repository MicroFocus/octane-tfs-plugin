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
using System;

namespace MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tfs
{
	public class TfsConfiguration
    {

        public Uri Uri{get; protected set; }
        public string Pat { get; protected set; }

        //public string UserName { get; protected set; }// = @"localhost\michael"; //= @"hpeswlab\_alm_octane_auto";
        public string Password { get; protected set; }// = "michael";

        public TfsConfiguration(Uri uri, string pat)
        {
            Uri = uri;
            Pat = pat;
        }


        public TfsConfiguration(Uri uri, string pat ,string password) :this(uri,pat)
        {
            Password = password;
        }
    }
}
