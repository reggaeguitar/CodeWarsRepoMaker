using System;

namespace CodeWarsRepoMaker
{
    class Program
    {
        const string BaseDir = @"c:\p";
        const string Username = "reggaeguitar";
        const string Orgname = "BlackCatEnterprises";

        static void Main()
        {
            try
            {
                MainImpl();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        static void MainImpl()
        {
            var input = new Input();
            var git = new Git();

            var inputArgs = input.GetInput();

            IRepoMaker repoMaker = new RepoMakerFactory().Create(inputArgs.Language);
           
            var dir = repoMaker.MakeRepo(BaseDir, inputArgs.RepoName, inputArgs.ImplClassName);

            git.MakeGitHubRepo(dir, inputArgs, Orgname, Username);

            // open vscode
            new PowershellRunner().RunCommandViaPS(dir, "code .");
            Console.WriteLine("Done successfully");
        }
    }
}
