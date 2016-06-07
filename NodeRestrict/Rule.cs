namespace DotSee.NodeRestrict
{
    /// <summary>
    /// Holds rules for automatically creating nodes.
    /// </summary>
    public class Rule
    {
        public string ParentDocType { get; private set; }
        public string ChildDocType { get; private set; }
        public int MaxNodes { get; private set; }
        public bool IncludeChildren { get; private set; }
        public bool ShowWarnings { get; private set; }
        public string CustomMessage { get; private set; }
        public string CustomMessageCategory { get; private set; }
       
        public Rule(string parentDocType, string childDocType, int maxNodes, bool includeChildren = false, bool showWarnings=false, string customMessage="", string customMessageCategory="" )
        {
            ParentDocType = parentDocType;
            ChildDocType = childDocType;
            MaxNodes = maxNodes;
            IncludeChildren = includeChildren;
            ShowWarnings = showWarnings;
            CustomMessage = customMessage;
            CustomMessageCategory = customMessageCategory;

           
        }
    }
}
