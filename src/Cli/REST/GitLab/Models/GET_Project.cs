using System.Text.Json.Serialization;

namespace gli.REST.GitLab;

public class GitLabProject
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("avatar_url")]
    public string AvatarUrl { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("name_with_namespace")]
    public string NameWithNamespace { get; set; }

    [JsonPropertyName("open_issues_count")]
    public int OpenIssuesCount { get; set; }

    [JsonPropertyName("builds_access_level")]
    public string BuildsAccessLevel { get; set; }

    [JsonPropertyName("snippets_access_level")]
    public string SnippetsAccessLevel { get; set; }

    [JsonPropertyName("resolve_outdated_diff_discussions")]
    public bool ResolveOutdatedDiffDiscussions { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("default_branch")]
    public string DefaultBranch { get; set; }

    [JsonPropertyName("public")]
    public bool Public { get; set; }

    [JsonPropertyName("path")]
    public string Path { get; set; }

    [JsonPropertyName("path_with_namespace")]
    public string PathWithNamespace { get; set; }

    [JsonPropertyName("issues_access_level")]
    public string IssuesAccessLevel { get; set; }

    [JsonPropertyName("merge_pipelines_enabled")]
    public bool MergePipelinesEnabled { get; set; }

    [JsonPropertyName("merge_requests_access_level")]
    public string MergeRequestsAccessLevel { get; set; }

    [JsonPropertyName("merge_method")]
    public string MergeMethod { get; set; }

    [JsonPropertyName("wall_enabled")]
    public bool WallEnabled { get; set; }

    [JsonPropertyName("wiki_access_level")]
    public string WikiAccessLevel { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("last_activity_at")]
    public DateTime LastActivityAt { get; set; }

    [JsonPropertyName("creator_id")]
    public long CreatorId { get; set; }

    [JsonPropertyName("ssh_url_to_repo")]
    public string SshUrl { get; set; }

    [JsonPropertyName("http_url_to_repo")]
    public string HttpUrl { get; set; }

    [JsonPropertyName("web_url")]
    public string WebUrl { get; set; }

    [JsonPropertyName("import_status")]
    public string ImportStatus { get; set; }

    [JsonPropertyName("import_error")]
    public string ImportError { get; set; }

    [JsonPropertyName("archived")]
    public bool Archived { get; set; }

    [JsonPropertyName("shared_runners_enabled")]
    public bool SharedRunnersEnabled { get; set; }

    [JsonPropertyName("group_runners_enabled")]
    public bool GroupRunnersEnabled { get; set; }

    [JsonPropertyName("topics")]
    public string[] Topics { get; set; }

    [JsonPropertyName("star_count")]
    public int StarCount { get; set; }

    [JsonPropertyName("forks_count")]
    public int ForksCount { get; set; }

    [JsonPropertyName("runners_token")]
    public string RunnersToken { get; set; }

    [JsonPropertyName("public_jobs")]
    public bool PublicJobs { get; set; }

    [JsonPropertyName("forked_from_project")]
    public GitLabProject ForkedFromProject { get; set; }

    [JsonPropertyName("repository_storage")]
    public string RepositoryStorage { get; set; }

    [JsonPropertyName("only_allow_merge_if_pipeline_succeeds")]
    public bool OnlyAllowMergeIfPipelineSucceeds { get; set; }

    [JsonPropertyName("only_allow_merge_if_all_discussions_are_resolved")]
    public bool OnlyAllowMergeIfDiscussionsAreResolved { get; set; }

    [JsonPropertyName("printing_merge_requests_link_enabled")]
    public bool PrintingMergeRequestsLinkEnabled { get; set; }

    [JsonPropertyName("request_access_enabled")]
    public bool RequestAccessEnabled { get; set; }

    [JsonPropertyName("approvals_before_merge")]
    public int ApprovalsBeforeMerge { get; set; }

    /// <summary>
    /// The maximum amount of time, in seconds, that a job can run.
    /// </summary>
    [JsonPropertyName("build_timeout")]
    public int? BuildTimeout { get; set; }

    [JsonPropertyName("lfs_enabled")]
    public bool LfsEnabled { get; set; }

    [JsonPropertyName("empty_repo")]
    public bool EmptyRepo { get; set; }

    [JsonPropertyName("mirror")]
    public bool Mirror { get; set; }

    [JsonPropertyName("mirror_user_id")]
    public long MirrorUserId { get; set; }

    [JsonPropertyName("mirror_trigger_builds")]
    public bool MirrorTriggerBuilds { get; set; }

    [JsonPropertyName("only_mirror_protected_branches")]
    public bool OnlyMirrorProtectedBranch { get; set; }

    [JsonPropertyName("mirror_overwrites_diverged_branches")]
    public bool MirrorOverwritesDivergedBranches { get; set; }

    [JsonPropertyName("mr_default_target_self")]
    public bool? MrDefaultTargetSelf { get; set; }

    [JsonPropertyName("ci_default_git_depth")]
    public int? CiDefaultGitDepth { get; set; }
}