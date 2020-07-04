using System;
using System.Linq;

namespace CodeWarsRepoMaker
{
    class Input
    {

        public InputArgs GetInput()
        {
            Console.WriteLine("Enter repo name");
            var repoName = Console.ReadLine();
            Console.WriteLine("Enter implementation class name");
            var implClassName = Console.ReadLine();
            Console.WriteLine("Enter proplem url");
            var problemUrl = Console.ReadLine();

            Console.WriteLine("Create github repo? (y/n)");
            var createGitHubRepo = Console.ReadLine().Trim().ToLower() == "y";

            string gitHubPassword = null;
            if (createGitHubRepo)
            {
                // todo mask or eat password characters the user enters
                Console.WriteLine("Enter github password");
                gitHubPassword = Console.ReadLine();
            }

            // todo dynamically get languages from Language enum
            Console.WriteLine("Enter language, supported languages are Ruby and FSharp");
            var languageInput = Console.ReadLine().ToUpper();
            var possibleLanguages = Enum.GetValues(typeof(Language)).OfType<string>().ToList();
            if (!possibleLanguages.Any(x => x.ToUpper() == languageInput))
            {
                Console.WriteLine("Please enter either Ruby or FSharp");
            }
            var language = Enum.Parse<Language>(possibleLanguages.Single(x => x.ToUpper() == languageInput));
            return new InputArgs(repoName, implClassName, problemUrl, createGitHubRepo, gitHubPassword, language);
        }
    }
}
