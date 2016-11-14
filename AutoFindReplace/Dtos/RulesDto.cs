namespace AutoFindReplace.Dtos
{
    public class RulesDto
    {
        public string FindWhat { get; set; }//1
        public string ReplaceWith { get; set; }//2
        public string FileName { get; set; }//3
        public string ProjectName { get; set; }//4
        public string SolutionName { get; set; }//5
        public bool Enabled { get; set; }//6
        public bool CaseSensitive { get; set; }//7
        public string Comments { get; set; }//8

        public string Summary//9
        {
            get
            {
                if (!Enabled)
                {
                    return "Rule is disabled." + NoChangeswillBeApplied;
                }
                else
                {
                    if (string.IsNullOrEmpty(FindWhat))
                    {
                        return "Find text not specified." + NoChangeswillBeApplied;
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(SolutionName))
                        {
                            return "Solution name not specified." + NoChangeswillBeApplied;
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(ProjectName))
                            {
                                return "Project name not specified." + NoChangeswillBeApplied;
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(FileName))
                                {
                                    return "File name not specified." + NoChangeswillBeApplied;
                                }
                                else
                                {
                                    var caseSensitive = CaseSensitive ? "case-sensitive" : "case-insensitive";
                                    return
                                            @"Whenever a solution named """ + SolutionName + @"""" +
                                            @" is opened, any occurrences of the text """ + FindWhat + @"""" +
                                            @" (" + caseSensitive + ")" +
                                            @" within the file """ + FileName + @"""" +
                                            @" in the project """ + ProjectName + @"""" +
                                            @" will be replaced with the text """ + ReplaceWith + @"""" +
                                            @", subject to any source-control restrictions.";
                                }
                            }
                        }
                    }
                }
            }
        }

        private const string NoChangeswillBeApplied = " No changes will be applied.";
    }
}
