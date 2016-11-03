using System.Windows.Forms;
using AutoFindReplace.Dtos;
using AutoFindReplace.Helpers;
using System;

namespace AutoFindReplace.Options
{
    public partial class RulesUserControl : UserControl
    {
        internal RulesOptions rulesOptions;
        private RulesDto rulesDto = new RulesDto();

        public RulesUserControl()
        {
            InitializeComponent();
            Initialize();
        }

        public void Initialize()
        {
            InitializeGridData();
            InitializeGridProperties();
        }

        private void InitializeGridProperties()
        {
            dataGridView1.AllowUserToAddRows = true;
            dataGridView1.AllowUserToDeleteRows = true;
            dataGridView1.AllowUserToResizeColumns = true;
            //TODO allow users to sort by clicking column heading
            //TODO columns to auto-expand to size of data
        }

        private void InitializeGridData()
        {
            // Must add columns first, then set the data source second
            AddColumns();
            SetDataSource();
        }

        private void AddColumns()
        {
            if (dataGridView1.Columns.Count == 0)
            {
                dataGridView1.Columns.Add(GetNewColumn(1, "text", "FindWhat", "Find", "The text to be searched for."));
                dataGridView1.Columns.Add(GetNewColumn(2, "text", "ReplaceWith", "Replace", "The new text to replace the \'Find\' text if found."));
                dataGridView1.Columns.Add(GetNewColumn(3, "text", "FileName", "File" + Environment.NewLine + "(inc. suffix)", "The name of the file to be search for the \'Find\' text." + Environment.NewLine + "The file suffix (e.g. .txt, .cs, .vb, .config, etc) must be included in this field." + Environment.NewLine + "This field is case-insensitive."));
                dataGridView1.Columns.Add(GetNewColumn(4, "text", "ProjectName", "Project" + Environment.NewLine + "(inc. suffix)", "The name of the project file containing the \'File\'." + Environment.NewLine + "The project file suffix (e.g. .csproj, .vbproj) must be included in this field." + Environment.NewLine + "This field is case-insensitive."));
                dataGridView1.Columns.Add(GetNewColumn(5, "text", "SolutionName", "Solution" + Environment.NewLine + "(inc. suffix)", "The name of the solution file containing the \'Project\'." + Environment.NewLine + "The file suffix (e.g. .sln) must be included in this field." + Environment.NewLine + "This field is case-insensitive."));
                dataGridView1.Columns.Add(GetNewColumn(6, "checkBox", "Enabled", "Enabled", "If this field is ticked the rule will be ignored and the \'File\' will not be updated." + Environment.NewLine + "Use this field to temporarily disable rule(s)."));
                dataGridView1.Columns.Add(GetNewColumn(7, "checkBox", "CaseSensitive", "Case Sensitive", "If this field is ticked the \'Find\' text will search using case-sensitivity." + Environment.NewLine + "If un-ticked the \'Find\' text will search using any case."));
                dataGridView1.Columns.Add(GetNewColumn(8, "text", "Comments", "Comments", "Your own notes, purely for your use, ignored by the extension."));
                dataGridView1.Columns.Add(GetNewColumn(9, "text", "Summary", "Summary", "A summary explanation of this rule.", true));
            }
        }

        private void SetDataSource()
        {
            var rulesDtos = RulesHelper.GetRulesDtos();
            var bindingSource = new BindingSource();
            bindingSource.DataSource = rulesDtos;
            dataGridView1.DataSource = bindingSource;
        }

        private DataGridViewColumn GetNewColumn(int displayIndex, string colType, string name, string headerText, string toolTipText, bool readOnly = false)
        {
            var minimumWidth = 2;

            switch (colType)
            {
                case "text":
                    return new DataGridViewTextBoxColumn
                    {
                        DataPropertyName = name,
                        DisplayIndex = displayIndex,
                        HeaderText = headerText,
                        MinimumWidth = minimumWidth,
                        Name = name,
                        ReadOnly = readOnly,
                        ToolTipText = toolTipText,
                        ValueType = typeof(string),
                        Visible = true,
                    };
                case "checkBox":
                    return new DataGridViewCheckBoxColumn
                    {
                        DataPropertyName = name,
                        DisplayIndex = displayIndex,
                        HeaderText = headerText,
                        MinimumWidth = minimumWidth,
                        Name = name,
                        ReadOnly = readOnly,
                        ToolTipText = toolTipText,
                        ValueType = typeof(string),
                        Visible = true,
                    };
                default:
                    return null;
            }
        }
    }
}