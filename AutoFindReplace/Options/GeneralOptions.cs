using AutoFindReplace.Helpers;
using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel;

namespace AutoFindReplace.Options
{
    public class GeneralOptions : DialogPage  
    {
        [Category(Constants.CategoryName)]
        [DisplayName("Allow files not under source-control to be updated")]
        [Description("By default only files under source control will be changed, but if this option is set to 'true' files that are not under source-control will also be updated.")]
        public bool AllowNonSourceControledFiles { get; set; } = false;

        internal bool UnderSourceControlOnly
        {
            get
            {
                return !AllowNonSourceControledFiles;
            }
        }

        [Category(Constants.CategoryName)]
        [DisplayName("Keep modified files open after updates")]
        [Description("If this option is set to 'true', any modified files will be opened in the Editor. To enable Undo, all files where a match is found must be opened for editing. You can then Undo the changes in each file.")]
        public bool KeepFileOpenAfterSave { get; set; } = false;

        [Category(Constants.CategoryName)]
        [DisplayName("Hide results dialog if no modifications made")]
        [Description("By default, when a solution is opened that has enabled rules, a result dialog appears showing a results summary, even if no modifications were made. If this option is set to 'true' the dialog will not be shown if the rule(s) resulted in no modifications.")]
        public bool HideResultsDialogIfNoModifications { get; set; } = false;
    }
}
