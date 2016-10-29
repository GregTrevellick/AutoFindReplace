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
        [Description("By default only files under source control will be changed, but if this option is checked files that are not under source-control will also be updated.")]
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
        [Description("If this option is ticked, any modified files will be opened in the Editor. To enable Undo, all files where a match is found must be opened for editing. You can then Undo the changes in each file.")]
        public bool KeepFileOpenAfterSave { get; set; } = false;
    }
}
