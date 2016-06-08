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
        public bool ShowWarnings { get; private set; }
        public string CustomMessage { get; private set; }
        public string CustomMessageCategory { get; private set; }
        public string CustomWarningMessage { get; private set; }
        public string CustomWarningMessageCategory { get; private set; }
        public bool FromProperty { get; private set; }

        public Rule(
            string parentDocType, 
            string childDocType, 
            int maxNodes,
            bool fromProperty = false, 
            bool showWarnings=false, 
            string customMessage="", 
            string customMessageCategory="", 
            string customWarningMessage = "", 
            string customWarningMessageCategory = "")
        {
            ParentDocType = parentDocType;
            ChildDocType = childDocType;
            MaxNodes = maxNodes;
            FromProperty = fromProperty;
            ShowWarnings = showWarnings;
            CustomMessage = customMessage;
            CustomMessageCategory = customMessageCategory;
            CustomWarningMessage = customWarningMessage;
            CustomWarningMessageCategory = customWarningMessageCategory;

        }

        public string GetMessage()
        {
            if (!string.IsNullOrEmpty(CustomMessage)) {return (CustomMessage);}

            if (FromProperty)
            {
                return (string.Format("Node saved but not published. Max allowed child nodes for this specific node: {0}.", MaxNodes.ToString()));
            }
            else
            {
                return string.Format(
                    "Node saved but not published. Max allowed nodes {1} directly under {2}: {0}."
                    , MaxNodes.ToString()
                    , ChildDocType.Equals("*") ? "of any type" : string.Format("of type \"{0}\"", ChildDocType)
                    , ParentDocType.Equals("*") ? "any node" : string.Format("nodes of type \"{0}\"", ParentDocType)
                    );
            }
            
        }
        public string GetMessageCategory()
        {
            return (string.IsNullOrEmpty(CustomMessageCategory) ? "Publish" : CustomMessageCategory);
        }

        public string GetWarningMessage(int currentNodeCount)
        {
            if (!string.IsNullOrEmpty(CustomWarningMessage)) { return (CustomWarningMessage); }

            if (FromProperty)
            {
                return (string.Format("Restrictions for this node are in place. You have published {0} out {1} allowed child nodes.", (currentNodeCount+1).ToString(), MaxNodes.ToString()));
            }
            else
            {
                return string.Format(
                    "Restrictions in place. {3} directly under {2}: {1} of {0} allowed."
                    , MaxNodes.ToString()
                    , (currentNodeCount + 1).ToString()
                    , ParentDocType.Equals("*") ? "any node" : string.Format("nodes of type \"{0}\"", ParentDocType)
                    , ChildDocType.Equals("*") ? "Any node" : string.Format("Nodes of type \"{0}\"", ChildDocType)
                    );
            }
        }
        public string GetWarningMessageCategory()
        {
            return (string.IsNullOrEmpty(CustomWarningMessageCategory) ? "Publish" : CustomWarningMessageCategory);
        }

    }
}
