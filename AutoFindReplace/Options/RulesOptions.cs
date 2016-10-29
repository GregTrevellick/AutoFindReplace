using System;
using System.Collections.Generic;
using System.Linq;
using AutoFindReplace.Dtos;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Settings;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Newtonsoft.Json;
using AutoFindReplace.Helpers;

namespace AutoFindReplace.Options
{
    [Guid("00000000-0000-0000-0000-000000000000")]
    public class RulesOptions : DialogPage
    {
        protected RulesUserControl userControl;
        private RulesDto rulesDto = new RulesDto();

        protected override IWin32Window Window
        {
            get
            {
                if (userControl == null)
                {
                    userControl = new RulesUserControl();
                    userControl.Initialize();
                    userControl.rulesOptions = this;
                }
                return userControl;
            }
        }

        public override void SaveSettingsToStorage()
        {
            base.SaveSettingsToStorage();

            var shellSettingsManager = new ShellSettingsManager(ServiceProvider.GlobalProvider);
            var writableSettingsStore = shellSettingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);

            if (writableSettingsStore.CollectionExists(Constants.CollectionPath))
            {
                writableSettingsStore.DeleteCollection(Constants.CollectionPath);
            }

            if (!writableSettingsStore.CollectionExists(Constants.CollectionPath))
            {
                writableSettingsStore.CreateCollection(Constants.CollectionPath);
            }

            var userControlWindow = (RulesUserControl)Window;
            var dgv = userControlWindow.Controls.Find(Constants.DataGridViewName, false).First() as DataGridView;
            var rulesDtos = new List<RulesDto>();

            var rows = dgv.Rows.Cast<DataGridViewRow>();

            for (int i = 0; i < rows.Count(); i++)
            {
                var rulesDto = new RulesDto();

                var dgvRow = dgv.Rows[i];

                foreach (DataGridViewCell cell in dgvRow.Cells)
                {
                    var owner = cell.OwningColumn;

                    switch (owner.DataPropertyName)
                    {
                        case "FindWhat":
                            rulesDto.FindWhat = (string)cell.EditedFormattedValue;
                            break;
                        case "ReplaceWith":
                            rulesDto.ReplaceWith = (string)cell.EditedFormattedValue;
                            break;
                        case "FileName":
                            rulesDto.FileName = (string)cell.EditedFormattedValue;
                            break;
                        case "SolutionName":
                            rulesDto.SolutionName = (string)cell.EditedFormattedValue;
                            break;
                        case "Comments":
                            rulesDto.Comments = (string)cell.EditedFormattedValue;
                            break;
                        case "ProjectName":
                            rulesDto.ProjectName = (string)cell.EditedFormattedValue;
                            break;
                        case "Enabled":
                            rulesDto.Enabled = (bool)cell.EditedFormattedValue;
                            break;
                        case "CaseSensitive":
                            rulesDto.CaseSensitive = (bool)cell.EditedFormattedValue;
                            break;
                        default:
                            break;
                    }
                }

                rulesDtos.Add(rulesDto);
            }

            var settingsStoreValue = JsonConvert.SerializeObject(rulesDtos);
            writableSettingsStore.SetString(Constants.CollectionPath, Constants.PropertyName, settingsStoreValue);
        }
    }
}