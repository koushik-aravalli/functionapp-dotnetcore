namespace Yodha.AzDevops.Api.ConfigurationOptions
{
    public class AzDevOpsOptions
    {
        public string PersonalAccessToken { get; set; }
        public string Organization { get; set; }
        public string Project { get; set; }
        public string RepositoryName { get; set; }
        public string BuildDefinitionId { get; set; }
        public string ReleaseDefinitionId { get; set; }
        public string SourceBranch { get; set; }
    }
}