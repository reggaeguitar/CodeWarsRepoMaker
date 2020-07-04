using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using System;

namespace CodeWarsRepoMaker
{
    class Git
    {
        public void MakeGitHubRepo(string dir, InputArgs inputArgs, string orgName, string username)
        {
            DoFirstCommit(dir);

            if (inputArgs.CreateGitHubRepo)
            {
                // create repo on github with description = $"Solution to {problemUrl}"
                CreateRepoWithSelenium(inputArgs.GitHubPassword, 
                    inputArgs.RepoName, inputArgs.ProblemUrl, username, orgName);
                // todo push seems to not be working, probably because of two factor authentication
                AddRemoteAndPush(dir, inputArgs.RepoName, orgName);
            }
        }

        private void AddRemoteAndPush(string directory, string repoName, string orgName)
        {
            // git remote add origin https://github.com/{Orgname}/{repoName}.git
            // git push -u origin master
            var gitUrl = $"https://github.com/{orgName}/{repoName}.git";
            RunGitCommand(directory, @"remote add origin " + gitUrl);
            RunGitCommand(directory, @"push -u origin master");
        }


        private void DoFirstCommit(string directory)
        {
            RunGitCommand(directory, @"init");
            RunGitCommand(directory, @"add .");
            RunGitCommand(directory, @"commit -am 'Initial commit'");
        }


        private void RunGitCommand(string directory, string command)
        {
            var fullCommand = $"git {command}";
            new PowershellRunner().RunCommandViaPS(directory, fullCommand);
        }

        private void CreateRepoWithSelenium(string password, string repoName, string problemUrl, string username, string orgName)
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
            stupidKasperskyPopup?.Click();

            ClickMainButtonOnPage();

            // verification code
            Console.WriteLine("Enter github verification code");
            var verificationCode = Console.ReadLine();

            IWebElement verificationCodeBox = driver.FindElement(By.Id("otp"));
            verificationCodeBox.SendKeys(verificationCode);

            ClickMainButtonOnPage();

            // get to org page
            driver.Navigate().GoToUrl($"https://github.com/{orgName}");
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
                ((IJavaScriptExecutor)driver).ExecuteScript($"window.scrollTo({element.Location.X},{element.Location.Y})");
                element.Click();
            }

            IWebElement GetMainButton() => driver.FindElement(By.ClassName(btnClass));

            driver.Close();
        }
    }
}
