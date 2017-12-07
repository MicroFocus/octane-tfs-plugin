using System;
using MicroFocus.Ci.Tfs.Octane;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MicroFocus.Ci.Tfs.Octane.Tfs.ApiItems;
using MicroFocus.Ci.Tfs.Octane.Tfs.Beans;
using MicroFocus.Ci.Tfs.Octane.Dto.TestResults;
using MicroFocus.Ci.Tfs.Octane.Dto.Scm;
using System.Collections.Generic;
using MicroFocus.Ci.Tfs.Octane.Tfs.Beans.v1.SCM;

namespace MicroFocus.Ci.Tfs.Tests
{
	[TestClass]
	public class TfsManagerTest : OctaneManagerBaseTest
	{
		[TestMethod]
		public void GetJobsList()
		{
			_tfsManager.GetJobsList();
		}

		[TestMethod]
		public void Test()
		{
			ScmData scmData = null;


			var changes = _tfsManager.GetBuildChanges("DefaultCollection", "3086f4e9-d2ef-4f1a-9e48-19bf30c794a5", "48");
			if (changes.Count > 0)
			{
				scmData = new ScmData();
				scmData.Commits = new List<ScmCommit>();
				foreach (TfsScmChange change in changes)
				{
					var commit = _tfsManager.GetCommitWithChanges(change.Location);
					commit.Changes = new List<TfsScmCommitChange>();
					ScmCommit scmCommit = new ScmCommit();
				

					if (scmData.Repository == null)
					{
						var repository = _tfsManager.GetRepository(commit.Links.Repository.Href);
						scmData.Repository = new ScmRepository();
						scmData.Repository.Branch = repository.DefaultBranch; //TODO find real branch
						scmData.Repository.Type = "git";//TODO find type 
						scmData.Repository.Url = repository.Url;
					}


				}
			}

			//return scmData;
		}
	}
}
