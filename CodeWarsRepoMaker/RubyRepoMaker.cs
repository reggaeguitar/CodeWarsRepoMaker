using System;
using System.IO;
using System.Text;

namespace CodeWarsRepoMaker
{
    public class RubyRepoMaker : IRepoMaker
    {
        const string TestNameTokenReplace = "${TestNameTokenReplace}";
        
        public string MakeRepo(string baseDir, string repoName, string implClassName)
        {
            var dir = Path.Join(baseDir, repoName);
            if (Directory.Exists(dir))
            {
                throw new Exception($"Directory {dir} already exists");
            }
            var rubyScaffoldDir = Path.Join(baseDir, "RubyScaffold");
            // copy everything from scaffold folder and rename
            new DirectoryCopier().CopyDirectoryAndAllContents(rubyScaffoldDir, dir);
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

        private static void ReplaceTestFileTokenInLaunchJson(string implClassName, string dir)
        {
            var path = Path.Combine(dir, @".vscode\launch.json");
            var text = File.ReadAllText(path);
            text = text.Replace(TestNameTokenReplace, implClassName + "Tests");
            File.WriteAllText(path, text);
        }
    }
}
