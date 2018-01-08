using log4net;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Dto.Events;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Dto.Scm;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tfs;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tfs.ApiItems;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tools
{
	public static class ScmEventHelper
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public static ScmData GetScmData(TfsApis tfsManager, TfsBuildInfo buildInfo)
		{
			try
			{
				ScmData scmData = null;
				var originalChanges = tfsManager.GetBuildChanges(buildInfo.CollectionName, buildInfo.Project, buildInfo.BuildId);
				if (originalChanges.Count > 0)
				{
					var build = tfsManager.GetBuild(buildInfo.CollectionName, buildInfo.Project, buildInfo.BuildId);
					ICollection<TfsScmChange> filteredChanges = GetFilteredBuildChanges(tfsManager, buildInfo, build, originalChanges);
					if (filteredChanges.Count > 0)
					{
						scmData = new ScmData();
						var repository = tfsManager.GetRepositoryById(buildInfo.CollectionName, build.Repository.Id);
						scmData.Repository = new ScmRepository();
						scmData.Repository.Branch = build.SourceBranch;
						scmData.Repository.Type = build.Repository.Type;
						scmData.Repository.Url = repository.RemoteUrl;

						scmData.BuiltRevId = build.SourceVersion;
						scmData.Commits = new List<ScmCommit>();
						foreach (TfsScmChange change in filteredChanges)
						{
							var tfsCommit = tfsManager.GetCommitWithChanges(change.Location);
							ScmCommit scmCommit = new ScmCommit();
							scmData.Commits.Add(scmCommit);
							scmCommit.User = tfsCommit.Committer.Name;
							scmCommit.UserEmail = tfsCommit.Committer.Email;
							scmCommit.Time = TestResultUtils.ConvertToOctaneTime(tfsCommit.Committer.Date);
							scmCommit.RevId = tfsCommit.CommitId;
							if (tfsCommit.Parents.Count > 0)
							{
								scmCommit.ParentRevId = tfsCommit.Parents[0];
							}

							scmCommit.Comment = tfsCommit.Comment;
							scmCommit.Changes = new List<ScmCommitFileChange>();

							foreach (var tfsCommitChange in tfsCommit.Changes)
							{
								if (!tfsCommitChange.Item.IsFolder)
								{
									ScmCommitFileChange commitChange = new ScmCommitFileChange();
									scmCommit.Changes.Add(commitChange);

									commitChange.Type = tfsCommitChange.ChangeType;
									commitChange.File = tfsCommitChange.Item.Path;
								}
							}
						}
					}

				}
				return scmData;

			}
			catch (Exception e)
			{
				Log.Error($"{buildInfo} - Failed to create scm data : {e.Message}");
				return null;
			}
		}

		/// <summary>
		/// Tfs returns associated changes from last successful build. That mean, for failed build it can return change that was reported for previous failed build.
		/// This method - clear previously reported changes of previous failed build
		/// </summary>
		private static ICollection<TfsScmChange> GetFilteredBuildChanges(TfsApis tfsManager, TfsBuildInfo buildInfo, TfsBuild build, ICollection<TfsScmChange> changes)
		{

			//put changes in map
			Dictionary<string, TfsScmChange> changesMap = new Dictionary<string, TfsScmChange>();
			foreach (TfsScmChange change in changes)
			{
				changesMap[change.Id] = change;
			}

			//find previous failed build
			IList<TfsBuild> previousBuilds = tfsManager.GetPreviousFailedBuilds(buildInfo.CollectionName, buildInfo.Project, build.StartTime);
			TfsBuild foundPreviousFailedBuild = null;
			foreach (TfsBuild previousBuild in previousBuilds)
			{
				//pick only build that done on the same branch
				if (build.SourceBranch.Equals(previousBuild.SourceBranch))
				{
					foundPreviousFailedBuild = previousBuild;
					break;
				}
			}

			if (foundPreviousFailedBuild != null)
			{
				//remove changes from previous build
				var previousChanges = tfsManager.GetBuildChanges(buildInfo.CollectionName, buildInfo.Project, foundPreviousFailedBuild.Id.ToString());
				foreach (TfsScmChange previousChange in previousChanges)
				{
					changesMap.Remove(previousChange.Id);
				}

				int removedCount = changes.Count - changesMap.Count;
				if (removedCount == 0)
				{
					Log.Debug($"{buildInfo} - build {build.Id} contains {changes.Count} associated changes. No one of them was already reported in previous build {foundPreviousFailedBuild.Id}");
				}
				else
				{
					Log.Debug($"{buildInfo} - build {build.Id} contains {changes.Count} associated changes while {removedCount} changes were already reported in build {foundPreviousFailedBuild.Id}");
				}
			}

			return changesMap.Values;

		}

	}
}
