using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Management.Automation;
using System.Text;

namespace CodeWarsRepoMaker
{
    class Program
    {
        const string BaseDir = @"c:\p";
        const string TestNameTokenReplace = "${TestNameTokenReplace}";
        const string Username = "reggaeguitar";
        const string Orgname = "BlackCatEnterprises";
        static readonly string RubyScaffoldDir = Path.Join(BaseDir, "RubyScaffold");


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

            Console.WriteLine("Create github repo? (y/n)");
            var createGitHubRepo = Console.ReadLine().Trim().ToLower() == "y";

            string githubPassword = null;
            if (createGitHubRepo)
            {
                // todo mask or eat password characters the user enters
                Console.WriteLine("Enter github password");
                githubPassword = Console.ReadLine();
            }

            //
            var dir = Path.Join(BaseDir, repoName);
            if (Directory.Exists(dir))
            {
                throw new Exception($"Directory {dir} already exists");
            }
            // copy everything from scaffold folder and rename
            CopyDirectoryAndAllContents(RubyScaffoldDir, dir);
            // replace token with test filename in launch.json
            ReplaceTestFileTokenInLaunchJson(implClassName, dir);
            // add implClass and test file
            using (FileStream fs = File.Create(Path.Combine(dir, $"{implClassName}.rb"))) { }
            using (FileStream fs = File.Create(Path.Combine(dir, $"{implClassName}Tests.rb"))) 
            {
                var firstLineOfFile = $"load \"{implClassName}.rb\"";
                var utf8 = new UTF8Encoding();
                byte[] asBytes = utf8.GetBytes(firstLineOfFile);
                fs.Write(asBytes);
            }

            // git stuff
            DoFirstCommit(dir);

            if (createGitHubRepo)
            {
                // create repo on github with description = $"Solution to {problemUrl}"
                CreateRepoWithSelenium(githubPassword, repoName, problemUrl);
                // todo push seems to not be working, probably because of two factor authentication
                AddRemoteAndPush(dir, repoName);
            }

            // open vscode
            RunCommandViaPS(dir, "code .");
            Console.WriteLine("Done successfully");
        }

        private static void AddRemoteAndPush(string directory, string repoName)
        {
            // git remote add origin https://github.com/{Username}/{repoName}.git
            // git push -u origin master
            var gitUrl = $"https://github.com/{Orgname}/{repoName}.git";
            RunGitCommand(directory, @"remote add origin " + gitUrl);
            RunGitCommand(directory, @"push -u origin master");
        }

        private static void DoFirstCommit(string directory)
        {
            RunGitCommand(directory, @"init");
            RunGitCommand(directory, @"add .");
            RunGitCommand(directory, @"commit -am 'Initial commit'");            
        }

        private static void RunGitCommand(string directory, string command)
        {
            var fullCommand = $"git {command}";
            RunCommandViaPS(directory, fullCommand);
        }

        private static void CreateRepoWithSelenium(string password, string repoName, string problemUrl)
        {
            const string btnClass = "btn-primary";
            IWebDriver driver = new FirefoxDriver
            {
                Url = "https://github.com/new"
            };            
            // login page
            IWebElement usernameBox = driver.FindElement(By.Id("login_field"));
            usernameBox.SendKeys(Username);

            IWebElement passwordBox = driver.FindElement(By.Id("password"));
            passwordBox.SendKeys(password);

            var stupidKasperskyPopup = driver.FindElement(By.TagName("iframe"));
            stupidKasperskyPopup.Click();

            ClickMainButtonOnPage();

            // verification code
            Console.WriteLine("Enter github verification code");
            var verificationCode = Console.ReadLine();

            IWebElement verificationCodeBox = driver.FindElement(By.Id("otp"));
            verificationCodeBox.SendKeys(verificationCode);

            ClickMainButtonOnPage();

            // get to org page
            driver.Navigate().GoToUrl($"https://github.com/{Orgname}");
            var classesOnNewRepoButton = "btn btn-primary d-flex flex-items-center flex-justify-center width-auto ml-md-3";
            var newRepoButton = driver.FindElement(By.CssSelector($"a[class='{classesOnNewRepoButton}']"));
            ScrollAndClick(newRepoButton);
            
            // now at new repo page
            IWebElement repoNameBox = driver.FindElement(By.Id("repository_name"));
            repoNameBox.SendKeys(repoName);

            IWebElement repoDescriptionBox = driver.FindElement(By.Id("repository_description"));
            var description = $"Solution to this {problemUrl}";
            repoDescriptionBox.SendKeys(description);

            // scroll down to see Create Repository button
            var createRepoButton = driver.FindElement(By.ClassName("first-in-line"));
            ScrollAndClick(createRepoButton);

            void ClickMainButtonOnPage() => GetMainButton().Click();

            void ScrollAndClick(IWebElement element)
            {
                //((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", element);
                ((IJavaScriptExecutor)driver).ExecuteScript($"window.scrollTo({element.Location.X},{element.Location.Y})");
                element.Click();
            }

            IWebElement GetMainButton() => driver.FindElement(By.ClassName(btnClass));
        }
        
        private static void RunCommandViaPS(string directory, string command)
        {
            using PowerShell powershell = PowerShell.Create();
            powershell.AddScript($"cd {directory}");
            powershell.AddScript(command);
            Collection<PSObject> results = powershell.Invoke();
        }

        private static void ReplaceTestFileTokenInLaunchJson(string implClassName, string dir)
        {
            var path = Path.Combine(dir, @".vscode\launch.json");
            var text = File.ReadAllText(path);
            text = text.Replace(TestNameTokenReplace, implClassName + "Tests");
            File.WriteAllText(path, text);
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
    }
}
