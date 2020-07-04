using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CodeWarsRepoMaker
{
    class Input
    {

        public InputArgs GetInput()
        {
            var inputText = File.ReadAllText("input.json");
            var inputArgs = JsonConvert.DeserializeObject<InputArgs>(inputText);
            if (inputArgs.CreateGitHubRepo)
            {
                // todo mask or eat password characters the user enters
                Console.WriteLine("Enter github password");
                inputArgs.GitHubPassword = Console.ReadLine();
            }
            return inputArgs;
        }
    }
}
