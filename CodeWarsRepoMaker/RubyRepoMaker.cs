using System;
using System.IO;
using System.Text;

namespace CodeWarsRepoMaker
{
    public class RubyRepoMaker : IRepoMaker
    {
        const string TestNameTokenReplace = "${TestNameTokenReplace}";
        public string BaseDir { get; private set; }

        public RubyRepoMaker(string baseDir)
        {
            BaseDir = baseDir;
        }

        public string MakeRepo(string repoName, string implClassName)
        {
            var dir = Path.Join(BaseDir, repoName);
            if (Directory.Exists(dir))
            {
                throw new Exception($"Directory {dir} already exists");
            }
            var rubyScaffoldDir = Path.Join(BaseDir, "RubyScaffold");
            // copy everything from scaffold folder and rename
            CopyDirectoryAndAllContents(rubyScaffoldDir, dir);
            // replace token with test filename in launch.json
            ReplaceTestFileTokenInLaunchJson(implClassName, dir);
            // add implClass and test file
            using (FileStream fs = File.Create(Path.Combine(dir, $"{implClassName}.rb"))) { }
            using (FileStream fs = File.Create(Path.Combine(dir, $"{implClassName}Tests.rb")))
            {
                var fileContents = $"load \"{implClassName}.rb\"";
                fileContents += Environment.NewLine + Environment.NewLine +
@"def assert_equals a, b
  puts a == b
end" + Environment.NewLine + Environment.NewLine;
                var utf8 = new UTF8Encoding();
                byte[] asBytes = utf8.GetBytes(fileContents);
                fs.Write(asBytes);
            }
            return dir;
        }

        // from tboswell's answer on
        // https://stackoverflow.com/questions/58744/copy-the-entire-contents-of-a-directory-in-c-sharp/58820
        private static void CopyDirectoryAndAllContents(string sourcePath, string destinationPath)
        {
            // Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*",
                SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(sourcePath, destinationPath));

            // Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(sourcePath, "*.*",
                SearchOption.AllDirectories))
                File.Copy(newPath, newPath.Replace(sourcePath, destinationPath), true);
        }

        private static void ReplaceTestFileTokenInLaunchJson(string implClassName, string dir)
        {
            var path = Path.Combine(dir, @".vscode\launch.json");
            var text = File.ReadAllText(path);
            text = text.Replace(TestNameTokenReplace, implClassName + "Tests");
            File.WriteAllText(path, text);
        }
    }
}
