using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MicroFocus.Ci.Tfs.Octane.dto;
using MicroFocus.Ci.Tfs.Octane.dto.general;
using MicroFocus.Ci.Tfs.Octane.dto.pipelines;

namespace MicroFocus.Ci.Tfs.Octane
{
    internal class TfsManager
    {
        private static readonly CiJobList MockJobList = new CiJobList();

        public TfsManager()
        {
            MockJobList.Jobs.Add(new PipelineNode("e7c63454-f81d-4786-8a33-cc1e7c9fa5ce", "Tfs Test Job"));
            MockJobList.Jobs.Add(new PipelineNode("f9e21042-a286-44bf-808a-8c9462cb3666", "Tfs Test Job 2"));
            MockJobList.Jobs.Add(new PipelineNode("d45f798c-ccf7-48dc-970d-3b320d63c75c", "Tfs Test Job 3"));

        }

        public IDtoBase GetJobsList()
        {
            //TODO: Change this to real code , currently a mockup!            

            Uri configurationServerUri = new Uri("http://your-server:8080/tfs");
            TfsConfigurationServer configurationServer =
                TfsConfigurationServerFactory.GetConfigurationServer(configurationServerUri);

            ITeamProjectCollectionService tpcService = configurationServer.GetService<ITeamProjectCollectionService>();

            foreach (TeamProjectCollection tpc in tpcService.GetCollections())
            {
                // Do your work for each TeamProjectCollection object here.
            }

            return MockJobList;
        }

        public IDtoBase GetJobDetail(string jobId)
        {
            return MockJobList[jobId];
        }
    }
}
