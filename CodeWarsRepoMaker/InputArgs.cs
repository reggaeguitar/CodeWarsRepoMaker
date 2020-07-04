namespace CodeWarsRepoMaker
{
    class InputArgs
    {
        public string RepoName { get; private set; }
        public string ImplClassName { get; private set; }
        public string ProblemUrl { get; private set; }
        public bool CreateGitHubRepo { get; private set; }
        public string GitHubPassword { get; set; }
        public Language Language { get; private set; }
        public InputArgs(string repoName, string implClassName, string problemUrl, bool createGitHubRepo, Language language)
        {
            RepoName = repoName;
            ImplClassName = implClassName;
            ProblemUrl = problemUrl;
            CreateGitHubRepo = createGitHubRepo;
            Language = language;
        }
       
    }
}
