using System;
using System.Windows.Forms;
using Newtonsoft.Json;
using AutoFindReplace.Helpers;

namespace AutoFindReplace.Options
{
    public partial class ExportRulesUserControl : UserControl
    {
        public ExportRulesUserControl()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var rulesDtos = RulesHelper.GetRulesDtos();
            var rulesForExport = JsonConvert.SerializeObject(rulesDtos);
            ExportRules(rulesForExport);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var rulesDtos = RulesHelper.GetRulesDtos();
            var rulesForExport = string.Empty;

            foreach (var rulesDto in rulesDtos)
            {
                var csvLine = string.Empty;
                csvLine += rulesDto.FindWhat + ",";
                csvLine += rulesDto.ReplaceWith + ",";
                csvLine += rulesDto.FileName + ",";
                csvLine += rulesDto.ProjectName + ",";
                csvLine += rulesDto.SolutionName + ",";
                csvLine += rulesDto.Enabled + ",";
                csvLine += rulesDto.CaseSensitive + ",";
                csvLine += rulesDto.Comments + Environment.NewLine;
                rulesForExport += csvLine;
            }

            ExportRules(rulesForExport);
        }

        private void ExportRules(string rulesForExport)
        {
            if (!string.IsNullOrEmpty(rulesForExport))
            {
                Clipboard.SetText(rulesForExport);
            }
        }
    }
}
