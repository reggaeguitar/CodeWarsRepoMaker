using System;
using System.IO;
using System.Linq;

namespace CodeWarsRepoMaker
{
    class FSharpRepoMaker : IRepoMaker
    {
        private const string ReplaceToken = @"${NamespaceNameTokenReplace}";
        private const string Placeholder = "template";

        public string MakeRepo(string baseDir, string repoName, string implClassName)
        {
            var dir = Path.Join(baseDir, repoName);
            if (Directory.Exists(dir))
            {
                throw new Exception($"Directory {dir} already exists");
            }
            var fsharpScaffoldDir = Path.Join(baseDir, "FSharpScaffold");
            // copy everything from scaffold folder and rename
            new DirectoryCopier().CopyDirectoryAndAllContents(fsharpScaffoldDir, dir);
            RenameFilesAndReplaceTokensInFiles(dir, repoName, implClassName);           
            return dir;
        }

        private void RenameFilesAndReplaceTokensInFiles(string dir, string repoName, string implClassName)
        {
            var solutionFile = Directory.GetFiles(dir).Single(x => x.Contains(".sln"));
            var newSolutionFile = solutionFile.Replace(Placeholder, repoName);
            File.Move(solutionFile, newSolutionFile);
            ReplaceTokensInFile(repoName, ReplaceToken, newSolutionFile);

            var templateFolder = Directory.GetDirectories(dir).Single();
            var newDir = templateFolder.Replace(Placeholder, repoName);
            Directory.Move(templateFolder, newDir);

            foreach (var file in Directory.GetFiles(newDir))
            {
                var newFile = file;
                if (file.Contains(Placeholder))
                {
                    newFile = file.Contains(".fsproj") ? 
                        file.Replace(Placeholder, repoName) : 
                        file.Replace(Placeholder, implClassName);
                    File.Move(file, newFile);
                }
                ReplaceTokensInFile(implClassName, ReplaceToken, newFile);
            }
        }

        private static void ReplaceTokensInFile(string token, string replaceToken, string file)
        {
            var text = File.ReadAllText(file);
            text = text.Replace(replaceToken, token);
            File.WriteAllText(file, text);
        }
    }
}
