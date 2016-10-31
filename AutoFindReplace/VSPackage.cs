using AutoFindReplace.Dtos;
using AutoFindReplace.Helpers;
using AutoFindReplace.Options;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace AutoFindReplace
{
    [ProvideAutoLoad(UIContextGuids80.SolutionExists)]
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About    
    [Guid("5e45aa4e-1a24-4edf-b10a-228b63448f70")]
    [ProvideOptionPage(typeof(GeneralOptions), Helpers.Constants.CategoryName, "General", 0, 0, true)]
    [ProvideOptionPage(typeof(RulesOptions), Helpers.Constants.CategoryName, "Rules", 0, 0, true)]
    [ProvideOptionPage(typeof(ExportRules), Helpers.Constants.CategoryName, "Export", 0, 0, true)]
    public sealed class VSPackage : Package
    {
        private int changesCount;
        private DTE dte;
        private IList<string> failureMessages;
        private bool anyRulesProcessed;
        private int rulesEnabledForThisSolutionCount;
        private int rulesProcesssedSuccessfullyCount;
        private int rulesProcesssedUnsuccessfullyCount;

        public VSPackage()
        {
        }

        protected override void Initialize()
        {
            base.Initialize();

            IServiceContainer serviceContainer = this as IServiceContainer;
            dte = serviceContainer.GetService(typeof(SDTE)) as DTE;
            var solutionEvents = dte.Events.SolutionEvents;
            solutionEvents.Opened += OnSolutionOpened;
        }

        private void OnSolutionOpened()
        {
            var messagesHelper = new MessagesHelper();

            failureMessages = new List<string>();
            IList<string> successMessages = new List<string>();
            IEnumerable<string> summaryMessages = new List<string>();

            var generalOptionsDto = GetGeneralOptionsDtoFromStorage();
            var rulesDtos = RulesHelper.GetRulesDtos();

            if (rulesDtos != null && rulesDtos.Any())
            {
                var applyChangesMessages = ApplyChangesToSourceCode(rulesDtos, generalOptionsDto, dte.Solution.FullName);
                foreach (var applyChangesMessage in applyChangesMessages)
                {
                    successMessages.Add(applyChangesMessage);
                }

                if (anyRulesProcessed)
                {
                    summaryMessages = messagesHelper.GetSummaryMessages(rulesEnabledForThisSolutionCount, rulesProcesssedSuccessfullyCount, rulesProcesssedUnsuccessfullyCount, changesCount);
                    var userFriendlySuccessMessages = messagesHelper.GetUserFriendlySuccessMessages(successMessages);
                    var popUpMessage = messagesHelper.GetPopUpMessage(failureMessages, userFriendlySuccessMessages, summaryMessages);
                    DisplayPopUpMessage(Helpers.Constants.CategoryName + " Results", popUpMessage);
                }
            }

            PostOpenProcessing();
        }

        private IEnumerable<string> ApplyChangesToSourceCode(IEnumerable<RulesDto> rulesDtos, GeneralOptionsDto generalOptionsDto, string dteSolutionFullName)
        {
            var applyChangesMessages = new List<string>();

            foreach (var rulesDto in rulesDtos.Where(x => x.Enabled))
            {
                try
                {
                    var haveWeOpenedTheCorrectSolution = HaveWeOpenedTheCorrectSolution(rulesDto, dteSolutionFullName);

                    if (haveWeOpenedTheCorrectSolution)
                    {
                        anyRulesProcessed = true;

                        rulesEnabledForThisSolutionCount++;

                        var targetFileFullPath = GetTargetFileFullPath(rulesDto, generalOptionsDto, dteSolutionFullName);

                        if (!string.IsNullOrEmpty(targetFileFullPath))
                        {
                            var findReplaceMessages = PerformFindReplace(rulesDto, generalOptionsDto, targetFileFullPath);
                            applyChangesMessages.AddRange(findReplaceMessages);
                        }

                        rulesProcesssedSuccessfullyCount++;
                    }
                }
                catch (Exception ex)
                {
                    failureMessages.Add(ex.Message);
                }
            }

            rulesProcesssedUnsuccessfullyCount = rulesEnabledForThisSolutionCount - rulesProcesssedSuccessfullyCount;
           
            return applyChangesMessages;
        }

        private string GetTargetFileFullPath(RulesDto rulesDto, GeneralOptionsDto generalOptionsDto, string dteSolutionFullName)
        {
            var actualSolutionPath = Path.GetDirectoryName(dteSolutionFullName);

            var actualProjectPath = GetProjectPathWithinSolutionDirectory(rulesDto.ProjectName, actualSolutionPath);

            if (!string.IsNullOrEmpty(actualProjectPath))
            {
                var actualFilePath = GetFilePathWithinProjectDirectory(rulesDto.FileName, actualProjectPath);

                if (!string.IsNullOrEmpty(actualFilePath))
                {
                    var targetFileIsUnderSourceControl = dte.SourceControl.IsItemUnderSCC(actualFilePath);

                    if (FileIsEligibleToBeChanged(rulesDto, generalOptionsDto, actualFilePath, targetFileIsUnderSourceControl))
                    {
                        if (File.Exists(actualFilePath))
                        {
                            return actualFilePath;
                        }
                        else
                        {
                            failureMessages.Add("File " + actualFilePath + " does not exist");
                            return string.Empty;
                        }
                    }
                }
            }

            return string.Empty;
        }

        private void PostOpenProcessing()
        {
            changesCount = 0;
            rulesEnabledForThisSolutionCount = 0;
            rulesProcesssedSuccessfullyCount = 0;
            rulesProcesssedUnsuccessfullyCount = 0;
        }

        private string GetProjectPathWithinSolutionDirectory(string projectName, string actualSolutionPath)
        {
            var projectFilePath = FindFilePathWithinParentPath(projectName, actualSolutionPath);

            if (!string.IsNullOrEmpty(projectFilePath))
            {
                var projectFolderPath = projectFilePath.TrimSuffix(projectName);
                return projectFolderPath;
            }
            else
            {
                return null;
            }
        }

        private string GetFilePathWithinProjectDirectory(string fileName, string actualProjectPath)
        {
            return FindFilePathWithinParentPath(fileName, actualProjectPath);
        }

        private string FindFilePathWithinParentPath(string fileToFind, string parentPath)
        {
            var matchingNameFiles = Directory.GetFiles(parentPath, fileToFind, SearchOption.AllDirectories);
            if (matchingNameFiles.Count() == 1)
            {
                return matchingNameFiles[0];
            }
            else
            {
                if (matchingNameFiles.Count() == 0)
                {
                    failureMessages.Add("File " + fileToFind + " cannot be found");
                }
                else
                {
                    failureMessages.Add("File " + fileToFind + " exists multiple times");
                }
                return null;
            }
        }

        private bool FileIsEligibleToBeChanged(RulesDto rulesDto, GeneralOptionsDto generalOptionsDto, string actualFilePath, bool targetFileIsUnderSourceControl)
        {
            bool fileIsEligibleToBeChanged;

            if (!string.IsNullOrEmpty(actualFilePath))
            {
                if (generalOptionsDto.UnderSourceControlOnly && !targetFileIsUnderSourceControl)
                {
                    failureMessages.Add("File " + rulesDto.FileName + " is not under source-control");
                    fileIsEligibleToBeChanged = false;
                }
                else
                {
                    fileIsEligibleToBeChanged = true;
                }
            }
            else
            {
                fileIsEligibleToBeChanged = false;
            }

            return fileIsEligibleToBeChanged;
        }

        private bool HaveWeOpenedTheCorrectSolution(RulesDto rulesDto, string dteSolutionFullName)
        {
            var actualSolutionPathArray = dteSolutionFullName.Split('\\');
            var actualSolutionName = actualSolutionPathArray.Last();

            return actualSolutionName.ToLower() == rulesDto.SolutionName.ToLower();
        }

        private GeneralOptionsDto GetGeneralOptionsDtoFromStorage()
        {
            var generalOptions = (GeneralOptions)GetDialogPage(typeof(GeneralOptions));

            var generalOptionsDto = new GeneralOptionsDto
            {
                UnderSourceControlOnly = generalOptions.UnderSourceControlOnly,
                KeepFileOpenAfterSave = generalOptions.KeepFileOpenAfterSave,
            };

            return generalOptionsDto;
        }

        private IEnumerable<string> PerformFindReplace(RulesDto rulesDto, GeneralOptionsDto generalOptionsDto, string targetFileFullPath)
        {
            var findReplaceMessages = new List<string>();

            dte.ItemOperations.OpenFile(targetFileFullPath);
            
            var textDocument = dte.ActiveDocument.Object("TextDocument") as TextDocument;

            var find = textDocument.DTE.Find;

            vsFindOptions vsFindOptions;

            if (rulesDto.CaseSensitive)
            {
                vsFindOptions = vsFindOptions.vsFindOptionsMatchCase;
            }
            else
            {
                vsFindOptions = vsFindOptions.vsFindOptionsFromStart;
            }

            var result = find.FindReplace(vsFindAction.vsFindActionReplaceAll, rulesDto.FindWhat, (int)vsFindOptions, rulesDto.ReplaceWith, vsFindTarget.vsFindTargetOpenDocuments);

            if (result == vsFindResult.vsFindResultReplaced)
            {
                changesCount++;
                findReplaceMessages.Add(rulesDto.FileName + " in " + rulesDto.ProjectName);
                dte.ActiveDocument.Save();
            }

            if (!generalOptionsDto.KeepFileOpenAfterSave)
            {
                dte.ActiveDocument.Close();
            }

            return findReplaceMessages;
        }

        private void DisplayPopUpMessage(string title, string text)
        {
            IVsUIShell uiShell = (IVsUIShell)GetService(typeof(SVsUIShell));
            Guid clsid = Guid.Empty;
            int result;
            uiShell.ShowMessageBox(
                0,
                ref clsid,
                title,
                text,
                string.Empty,
                0,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
                OLEMSGICON.OLEMSGICON_INFO,
                0,
                out result);
        }
    }
}
