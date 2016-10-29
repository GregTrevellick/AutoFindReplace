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
        private static bool applyTestRules = false;

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
            rulesDtos.RemoveAll(x => string.IsNullOrEmpty(x.SolutionName));
            rulesDtos.RemoveAll(x => string.IsNullOrEmpty(x.ProjectName));
            rulesDtos.RemoveAll(x => string.IsNullOrEmpty(x.FileName));
        }

        private static void ApplyTestingRules(List<RulesDto> rulesDtos)
        {
            var testingRuleSolutionName = "DummySol.sln";
            rulesDtos.RemoveAll(x => x.SolutionName == testingRuleSolutionName);

            var proj1 = "DummySubFolder.DummyProj.csproj";
            var testingRule1 = new RulesDto
            {
                CaseSensitive = false,
                Enabled = true,
                ProjectName = proj1,
                FileName = "Dummy.txt",
                FindWhat = "foo",
                ReplaceWith = "bar",
                Comments = "Test rule 1",
                SolutionName = testingRuleSolutionName,
            };

            var testingRule2 = new RulesDto
            {
                CaseSensitive = false,
                Enabled = false,
                ProjectName = proj1,
                FileName = "Dummy.txt",
                FindWhat = "hello",
                ReplaceWith = "world",
                Comments = "Test rule 2",
                SolutionName = testingRule1.SolutionName,
            };

            rulesDtos.Add(testingRule1);
            rulesDtos.Add(testingRule2);
        }
    }
}
