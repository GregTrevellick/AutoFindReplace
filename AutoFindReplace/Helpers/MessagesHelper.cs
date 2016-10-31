using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoFindReplace.Helpers
{
    public class MessagesHelper
    {
        private string popUpMessage;

        public IEnumerable<string> GetSummaryMessages(int rulesEnabledForThisSolutionCount, int rulesProcesssedSuccessfullyCount, int rulesProcesssedUnsuccessfullyCount, int changesCount)
        {
            var summaryMessages = new List<string>();

            summaryMessages.Add("Rules enabled for this solution:  " + rulesEnabledForThisSolutionCount);
            summaryMessages.Add("Rules successfully processsed:    " + rulesProcesssedSuccessfullyCount);
            summaryMessages.Add("Rules un-successfully processsed: " + rulesProcesssedUnsuccessfullyCount);
            summaryMessages.Add("Number of files updated: " + changesCount);

            return summaryMessages;
        }

        public string GetPopUpMessage(IEnumerable<string> failureMessages, IEnumerable<string> successMessages, IEnumerable<string> summaryMessages)
        {
            popUpMessage = string.Empty;

            PopulatePopUpMessage(summaryMessages);
            PopulatePopUpMessage(successMessages, "FILES SUCCESSFULLY CHANGED");
            PopulatePopUpMessage(failureMessages, "UNSUCCESSFULL UPDATES LOG");

            return popUpMessage;
        }

        private void PopulatePopUpMessage(IEnumerable<string> messages, string title = null)
        {
            if (messages != null)
            {
                if (messages.Count() > 0 && !string.IsNullOrEmpty(title))
                {
                    popUpMessage += title + Environment.NewLine;
                }

                foreach (var message in messages)
                {
                    popUpMessage += message + Environment.NewLine;
                }
            }

            popUpMessage += Environment.NewLine;
        }

        public IEnumerable<string> GetUserFriendlySuccessMessages(IEnumerable<string> successMessages)
        {
            return successMessages.Distinct().OrderBy(x => x);
        }
    }
}
