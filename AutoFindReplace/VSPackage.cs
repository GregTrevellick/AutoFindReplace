using AutoFindReplace.Dtos;
using AutoFindReplace.Helpers;
using AutoFindReplace.Options;
using EnvDTE;
using EnvDTE80;
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
        private bool anyRulesProcessed;
        private int changesCount;
        private DTE dte;
        private IList<string> failureMessages;
        private Dictionary<string, string> projectPaths;
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

                if (anyRulesProcessed || failureMessages.Count > 0)
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
                        projectPaths = new Dictionary<string, string>();
                        SetProjectPaths();
                        anyRulesProcessed = true;
                        rulesEnabledForThisSolutionCount++;

                        var targetFileFullPath = GetTargetFileFullPath(rulesDto, generalOptionsDto);

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

        private void SetProjectPaths()
        {
            for (int i = 0; i < dte.Solution.Projects.Count; i++)
            {
                var item = dte.Solution.Projects.Item(i + 1);
                if (item.Name != "Solution Items")
                {
                    if (string.IsNullOrEmpty(item.FullName))
                    {
                        for (int j = 0; j < item.ProjectItems.Count; j++)
                        {
                            var projectItem = item.ProjectItems.Item(j + 1);
                            var projectPath = Path.GetDirectoryName(projectItem.Name);
                            var projectName = projectItem.Name.TrimPrefix(projectPath).TrimPrefix(@"\");
                            projectPaths.Add(projectName, projectPath);
                        }
                    }
                    else
                    {
                        var projectPath = Path.GetDirectoryName(item.FullName);
                        var projectName = item.FullName.TrimPrefix(projectPath).TrimPrefix(@"\");
                        projectPaths.Add(projectName, projectPath);
                    }
                }
            }

            //var fullNames = new List<string>();
            //var sol = dte.Solution;
            //var projs = sol.Projects;

            ////////DTE2 dte2 = Package.GetGlobalService(typeof(DTE)) as DTE2;
            ////////var sol = dte2.Solution;
            ////////var projs = sol.Projects;
            //foreach (var proj in sol)//eh - just 1 item in 'sol' surely ? shouldn't this be 'var proj in projs' ?
            //{
            //    var fullName = string.Empty;

            //    var project = proj as Project;
            //    if (project.Kind == ProjectKinds.vsProjectKindSolutionFolder)
            //    {
            //        var innerProjects = GetSolutionFolderProjects(project);
            //        foreach (var innerProject in innerProjects)
            //        {
            //            //carry out actions here.
            //        }
            //    }

            //    if (!string.IsNullOrEmpty(fullName))
            //    {
            //        fullNames.Add(fullName);
            //    }
            //}
            ////////}

            //foreach(var fullName in fullNames)
            //{
            //    var projectPath = Path.GetDirectoryName(fullName);
            //    var projectName = fullName.TrimPrefix(projectPath).TrimPrefix(@"\");
            //    projectPaths.Add(projectName, projectPath);
            //}
        }

        //private IEnumerable<Project> GetSolutionFolderProjects(Project project)
        //{
        //    List<Project> projects = new List<Project>();
        //    var y = (project.ProjectItems as ProjectItems).Count;
        //    for (var i = 1; i <= y; i++)
        //    {
        //        var x2 = project.ProjectItems.Item(i);

        //        if (project.Kind == ProjectKinds.vsProjectKindSolutionFolder)
        //        {
        //            //???
        //            var innerProjects2  = GetSolutionFolderProjects2(project);
        //        }
        //        else
        //        {
        //            var x = x2.SubProject;
        //            var subProject = x as Project;
        //            if (subProject != null)
        //            {
        //                var projectPath = Path.GetDirectoryName(x.FullName);
        //                var projectName = x.FullName.TrimPrefix(projectPath).TrimPrefix(@"\");
        //                projectPaths.Add(projectName, projectPath);
        //            }
        //        }
        //    }

        //    return projects;
        //}

        //private IEnumerable<Project> GetSolutionFolderProjects2(Project project)
        //{
        //    List<Project> projects = new List<Project>();
        //    var y = (project.ProjectItems as ProjectItems).Count;
        //    for (var i = 1; i <= y; i++)
        //    {
        //        var x2 = project.ProjectItems.Item(i);

        //        if (project.Kind == ProjectKinds.vsProjectKindSolutionFolder)
        //        {
        //            //???
        //        }
        //        else
        //        {
        //            var x = x2.SubProject;
        //            var subProject = x as Project;
        //            if (subProject != null)
        //            {
        //                var projectPath = Path.GetDirectoryName(x.FullName);
        //                var projectName = x.FullName.TrimPrefix(projectPath).TrimPrefix(@"\");
        //                projectPaths.Add(projectName, projectPath);
        //            }
        //        }
        //    }

        //    return projects;
        //}

        //gregt
        private string GetTargetFileFullPath(RulesDto rulesDto, GeneralOptionsDto generalOptionsDto)
        {
            var actualProjectPath = GetProjectPath(rulesDto.ProjectName);

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

        //gregt
        private string GetProjectPath(string projectName)
        {
            var projectFilePath = projectPaths.Where(x => x.Key == projectName).FirstOrDefault().Value;

            if (!string.IsNullOrEmpty(projectFilePath))
            {
                return projectFilePath;
            }
            else
            {
                return null;
            }
        }

        //gregt
        private string GetFilePathWithinProjectDirectory(string fileName, string actualProjectPath)
        {
            return FindFilePathWithinParentPath(fileName, actualProjectPath);
        }

        //gregt
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

            if (textDocument == null)
            {
                failureMessages.Add("File " + rulesDto.FileName + " is not an editable text file");
            }
            else
            {
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
                    findReplaceMessages.Add(rulesDto.FileName.ToLower() + " in " + rulesDto.ProjectName.ToLower());
                    dte.ActiveDocument.Save();
                }
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
                title.ToUpper(),
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
