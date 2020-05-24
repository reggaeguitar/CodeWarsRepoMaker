using System;
using System.IO;

namespace CodeWarsRepoMaker
{
    class Program
    {
        static readonly string BaseDir = @"c:\p";
        static readonly string RubyScaffoldDir = Path.Join(BaseDir, "RubyScaffold");
        static readonly string TestNameTokenReplace = "${TestNameTokenReplace}";

        static void Main(string[] args)
        {
            try
            {
                MainImpl();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static void MainImpl()
        {
            Console.WriteLine("Enter repo name");
            var repoName = Console.ReadLine();
            Console.WriteLine("Enter implementation class name");
            var implClassName = Console.ReadLine();
            Console.WriteLine("Enter proplem url");
            var problemUrl = Console.ReadLine();

            //
            var dir = Path.Join(BaseDir, repoName);
            if (Directory.Exists(dir))
            {
                throw new Exception($"Directory {dir} already exists");
            }
            // copy everything from scaffold folder and rename
            CopyDirectoryAndAllContents(RubyScaffoldDir, dir);
            // replace token with test filename in launch.json
            ReplaceTestFileToken(implClassName, dir);
            // add implClass and test file
            File.Create(Path.Combine(dir, $"{implClassName}.rb"));
            File.Create(Path.Combine(dir, $"{implClassName}Tests.rb"));
            // git init
            // git add .
            // git commit -am "Initial commit"
            // create repo on github with description = $"Solution to {problemUrl}"
            // git remote add origin githubUrl
            // git push -u origin master
            Console.WriteLine("Done successfully");
        }

        private static void ReplaceTestFileToken(string implClassName, string dir)
        {
            var path = Path.Combine(dir, @".vscode\launch.json");
            var text = File.ReadAllText(path);
            text = text.Replace(TestNameTokenReplace, implClassName + "Tests");
            File.WriteAllText(path, text);
        }

        private static void CopyDirectoryAndAllContents(string sourcePath, string destinationPath)
        {
            //Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*",
                SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(sourcePath, destinationPath));

            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(sourcePath, "*.*",
                SearchOption.AllDirectories))
                File.Copy(newPath, newPath.Replace(sourcePath, destinationPath), true);
        }
    }
}
