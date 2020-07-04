using System;
using System.Collections.Generic;
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

            // get language
            // todo dynamically get languages from Language enum
            Console.WriteLine("Enter language, supported languages are Ruby and FSharp");
            var languageInput = Console.ReadLine().ToUpper();
            var languages = Enum.GetValues(typeof(Language));
            var possibleLanguages = new List<string>();
            foreach (var lang in languages)
            {
                possibleLanguages.Add(lang.ToString());
            }
            if (!possibleLanguages.Any(x => x.ToUpper() == languageInput))
            {
                Console.WriteLine("Please enter either Ruby or FSharp");
            }
            var languageMatch = possibleLanguages.Single(x => x.ToUpper() == languageInput);
            var language = Enum.Parse<Language>(languageMatch);

            // github repo
            Console.WriteLine("Create github repo? (y/n)");
            var createGitHubRepo = Console.ReadLine().Trim().ToLower() == "y";

            string gitHubPassword = null;
            if (createGitHubRepo)
            {
                // todo mask or eat password characters the user enters
                Console.WriteLine("Enter github password");
                gitHubPassword = Console.ReadLine();
            }
            return new InputArgs(repoName, implClassName, problemUrl, createGitHubRepo, gitHubPassword, language);
        }
    }
}
