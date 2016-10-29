using System;
using Microsoft.VisualStudio.Shell;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace AutoFindReplace.Options
{
    [Guid("00000000-0000-0000-0000-000000000000")]
    public class ExportRules : DialogPage
    {
        protected ExportRulesUserControl userControl;

        protected override IWin32Window Window
        {
            get
            {
                if (userControl == null)
                {
                    userControl = new ExportRulesUserControl();
                }
                return userControl;
            }
        }
    }
}