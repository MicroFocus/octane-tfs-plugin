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
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Octane;

namespace MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Dto.General
{
	public class TfsCiEntity
    {
        public string CollectionName { get; }
        public string ProjectId { get; }
        public string BuildDefId { get; }

        public TfsCiEntity(string collectionName, string projectId, string buildDefId)
        {
            ProjectId = projectId;
            BuildDefId = buildDefId;
        }

        public override string ToString()
        {
            return OctaneUtils.GenerateOctaneJobCiId(CollectionName, ProjectId, BuildDefId);
        }
        
    }
}
