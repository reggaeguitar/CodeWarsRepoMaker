﻿using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Management.Automation;
using System.Net.Http;
using System.Security.AccessControl;
using System.Text;

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
            Console.WriteLine("Enter github password");
            var githubPassword = Console.ReadLine();
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

            using (FileStream fs = File.Create(Path.Combine(dir, $"{implClassName}.rb"))) { }
            using (FileStream fs = File.Create(Path.Combine(dir, $"{implClassName}Tests.rb"))) { }
            // git stuff
            DoFirstCommit(dir);
            // create repo on github with description = $"Solution to {problemUrl}"
            CreateRepoWithSelenium("reggaeguitar", githubPassword, repoName, problemUrl);           
            AddRemoteAndPush(dir, repoName);
            // open vscode
            RunCommandViaPS(dir, "code .");
            Console.WriteLine("Done successfully");
        }

        private static void AddRemoteAndPush(string directory, string repoName)
        {
            //git remote add origin https://github.com/reggaeguitar/repoTestCreation.git
            //git push -u origin master
            var gitUrl = $"https://github.com/reggaeguitar/{repoName}.git";
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

        private static void CreateRepoWithSelenium(string username, string password, string repoName, string problemUrl)
        {
            const string btnClass = "btn-primary";
            IWebDriver driver = new FirefoxDriver
            {
                Url = "https://github.com/new"
            };            
            // login page
            IWebElement usernameBox = driver.FindElement(By.Id("login_field"));
            usernameBox.SendKeys(username);

            IWebElement passwordBox = driver.FindElement(By.Id("password"));
            passwordBox.SendKeys(password);

            var stupidKasperskyPopup = driver.FindElement(By.TagName("iframe"));
            stupidKasperskyPopup.Click();

            clickMainButtonOnPage();

            // verification code
            Console.WriteLine("Enter github verification code");
            var verificationCode = Console.ReadLine();

            IWebElement verificationCodeBox = driver.FindElement(By.Id("otp"));
            verificationCodeBox.SendKeys(verificationCode);

            clickMainButtonOnPage();

            // new repo page
            IWebElement repoNameBox = driver.FindElement(By.Id("repository_name"));
            repoNameBox.SendKeys(repoName);

            IWebElement repoDescriptionBox = driver.FindElement(By.Id("repository_description"));
            var description = $"Solution to this{problemUrl}";
            repoDescriptionBox.SendKeys(description);

            //clickMainButtonOnPage();
            var element = driver.FindElement(By.ClassName("first-in-line"));
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", element);
            element.Click();

            void clickMainButtonOnPage()
            {
                driver.FindElement(By.ClassName(btnClass)).Click();
            }
        }
        
        private static void RunCommandViaPS(string directory, string command)
        {
            using PowerShell powershell = PowerShell.Create();
            powershell.AddScript($"cd {directory}");
            powershell.AddScript(command);
            Collection<PSObject> results = powershell.Invoke();
        }

        private static void ReplaceTestFileToken(string implClassName, string dir)
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
