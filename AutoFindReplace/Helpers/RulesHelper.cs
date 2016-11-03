using System.Collections.Generic;
using AutoFindReplace.Dtos;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Settings;
using Newtonsoft.Json;
using System;

namespace AutoFindReplace.Helpers
{
    public static class RulesHelper 
    {
        private static RulesDto rulesDto = new RulesDto();
        private static string settingsInStore;
        private static bool applyTestRules = true; //gregt set to false

        public static List<RulesDto> GetRulesDtos()
        {
            var shellSettingsManager = new ShellSettingsManager(ServiceProvider.GlobalProvider);
            var writableSettingsStore = shellSettingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);

            if (!writableSettingsStore.CollectionExists(Constants.CollectionPath))
            {
                return null;
            }

            try
            {
                settingsInStore = writableSettingsStore.GetString(Constants.CollectionPath, Constants.PropertyName);
            }
            catch (Exception ex)
            {
                settingsInStore = string.Empty;
                throw ex;
            }

            var rulesDtos = JsonConvert.DeserializeObject<List<RulesDto>>(settingsInStore);

            RemoveIncompleteRules(rulesDtos);

            if (applyTestRules)
            {
                ApplyTestingRules(rulesDtos);
            }

            return rulesDtos;
        }

        private static void RemoveIncompleteRules(List<RulesDto> rulesDtos)
        {
            rulesDtos.RemoveAll(x => string.IsNullOrEmpty(x.FindWhat));
            rulesDtos.RemoveAll(x => string.IsNullOrEmpty(x.FileName));
            rulesDtos.RemoveAll(x => string.IsNullOrEmpty(x.ProjectName));
            rulesDtos.RemoveAll(x => string.IsNullOrEmpty(x.SolutionName));
        }

        private static void ApplyTestingRules(List<RulesDto> rulesDtos)
        {
            //TODO add a flag to differentiate these test rules with a user's real rules

            var solutionNames = new List<string>
            {
                "Any.Sln",
                "AWebApp.sln",
                "dummy",
                "Dummy.Sln",
                "IDontExist.Sln",
                "JoePublic.Sln",
                "Orwell.Sln",
                "WebApp.sln",//gregt can be deleted 
            };

            // Remove any existant test rules
            foreach (var solutionName in solutionNames)
            {
                rulesDtos.RemoveAll(x => x.SolutionName.ToLower() == solutionName.ToLower());
            }

            #region Add new rules
            rulesDtos.Add (new RulesDto
            {
                FindWhat = "127.0.0.1",
                ReplaceWith = "localhost",
                FileName = "app.config",
                ProjectName = "JaneDoe.vbproj",
                SolutionName = "JoePublic.Sln",
                Enabled = true,
                CaseSensitive = false,
                Comments = "Connection string example perhaps"
            });

            rulesDtos.Add(new RulesDto
            {
                FindWhat = "Todo",
                ReplaceWith = "TODO",
                FileName = "myclass.cs",
                ProjectName = "myproj.csproj",
                SolutionName = "JoePublic.Sln",
                Enabled = false,
                CaseSensitive = true,
                Comments = "Make those TODOs consistent !"
            });

            rulesDtos.Add(new RulesDto
            {
                FindWhat = "hello",
                ReplaceWith = "world",
                FileName = "MyClass.cs",
                ProjectName = "MyProj.csproj",
                SolutionName = "JoePublic.Sln",
                Enabled = true,
                CaseSensitive = true,
                Comments = "Simple example"
            });

            rulesDtos.Add(new RulesDto
            {
                FindWhat = "Hello",
                ReplaceWith = "World",
                FileName = "MyClass.cs",
                ProjectName = "MyProj.csproj",
                SolutionName = "JoePublic.Sln",
                Enabled = true,
                CaseSensitive = true,
                Comments = "Another simple example"
            });

            rulesDtos.Add(new RulesDto
            {
                FindWhat = "foo",
                ReplaceWith = "bar",
                FileName = "Duplicate.Any",
                ProjectName = "MyProj.csproj",
                SolutionName = "JoePublic.Sln",
                Enabled = true,
                CaseSensitive = false,
                Comments = "This won't work - there are two Duplicate.Any files on disc"
            });

            rulesDtos.Add(new RulesDto
            {
                FindWhat = "© 1984",
                ReplaceWith = "© 2016",
                FileName = "RssFeed.Xml",
                ProjectName = "George.Csproj",
                SolutionName = "Orwell.Sln",
                Enabled = false,
                CaseSensitive = false,
                Comments = "One for luck - a disabled rule"
            });

            rulesDtos.Add(new RulesDto
            {
                FindWhat = "copyright 2015",
                ReplaceWith = "Copyright 2016",
                FileName = "Licence.txt",
                ProjectName = "any1.csproj",
                SolutionName = "any.sln",
                Enabled = true,
                CaseSensitive = false,
                Comments = "Bring your C# copyrights up to date"
            });

            rulesDtos.Add(new RulesDto
            {
                FindWhat = "cOpYrIgHt 2015",
                ReplaceWith = "Copyright 2016",
                FileName = "LiCeNcE.tXt",
                ProjectName = "aNy2.vBpRoJ",
                SolutionName = "aNy.sLn",
                Enabled = true,
                CaseSensitive = false,
                Comments = "Same for VB copyrights - casing doesn't matter !"
            });

            rulesDtos.Add(new RulesDto
            {
                FindWhat = "often transopsed",
                ReplaceWith = "often transposed",
                FileName = "dummy.txt",
                ProjectName = "dummy.csproj",
                SolutionName = "dummy.sln",
                Enabled = true,
                CaseSensitive = false,
                Comments = "Solve your colleague's frequent transposing"
            });

            rulesDtos.Add(new RulesDto
            {
                FindWhat = string.Empty,
                ReplaceWith = "Bart",
                FileName = "dummy.txt",
                ProjectName = "dummy.csproj",
                SolutionName = "dummy.sln",
                Enabled = true,
                CaseSensitive = false,
                Comments = "This won't work - missing find text"
            });

            rulesDtos.Add(new RulesDto
            {
                FindWhat = "Homer",
                ReplaceWith = string.Empty,
                FileName = "dummy.txt",
                ProjectName = "dummy.csproj",
                SolutionName = "dummy.sln",
                Enabled = true,
                CaseSensitive = false,
                Comments = "This will work - Homer becomes blank (no change there then !)"
            });

            rulesDtos.Add(new RulesDto
            {
                FindWhat = "Homer",
                ReplaceWith = "Bart",
                FileName = "dummy",
                ProjectName = "dummy.csproj",
                SolutionName = "dummy.sln",
                Enabled = true,
                CaseSensitive = false,
                Comments = "This won't work - missing file suffix"
            });

            rulesDtos.Add(new RulesDto
            {
                FindWhat = "Homer",
                ReplaceWith = "Bart",
                FileName = "dummy.txt",
                ProjectName = "dummy",
                SolutionName = "dummy.sln",
                Enabled = true,
                CaseSensitive = false,
                Comments = "This won't work - missing project suffix"
            });

            rulesDtos.Add(new RulesDto
            {
                FindWhat = "Homer",
                ReplaceWith = "Bart",
                FileName = "dummy.txt",
                ProjectName = "dummy.csproj",
                SolutionName = "dummy",
                Enabled = true,
                CaseSensitive = false,
                Comments = "This won't work - missing solution suffix"
            });

            rulesDtos.Add(new RulesDto
            {
                FindWhat = "Marge",
                ReplaceWith = "Lisa",
                FileName = "IDontExist.Txt",
                ProjectName = "IDontExist.Csproj",
                SolutionName = "IDontExist.Sln",
                Enabled = true,
                CaseSensitive = false,
                Comments = "None of these will work - the file / project / solution don't exist on disc"
            });

            rulesDtos.Add(new RulesDto
            {
                FindWhat = "<title>",
                ReplaceWith = "<title>W-I-P&nbsp;",
                FileName = "Site.Master",
                ProjectName = "AWebApp.csproj",
                SolutionName = "AWebApp.sln",
                Enabled = false,
                CaseSensitive = false,
                Comments = "Another example - ensure your local screenshots reflect WIP"
            });
            #endregion 
        }
    }
}
