namespace DotSee.NodeRestrict
{
    /// <summary>
    /// Holds rules for restricting node publishing.
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

        /// <summary>
        /// Holds the data for a node restriction rule.
        /// </summary>
        /// <param name="parentDocType">The parent document type alias</param>
        /// <param name="childDocType">The child document type alias (that is, the document being published)</param>
        /// <param name="maxNodes">The maximum number of child nodes allowed for the parent document type alias</param>
        /// <param name="fromProperty">Indicates whether the rule has been created on the fly based on the document's special property (true) or is a rule that comes from the config file (false)</param>
        /// <param name="showWarnings">If true, a warning message will be displayed when a node is published and a rule is in effect, if the limit has not been reached</param>
        /// <param name="customMessage">A custom "limit reached" message. This overrides the standard message.</param>
        /// <param name="customMessageCategory">Custom category for the "limit reached" message. This overrides the standard category literal.</param>
        /// <param name="customWarningMessage">A custom warning message. This overrides the standard message.</param>
        /// <param name="customWarningMessageCategory">Custom category for the warning message. This overrides the standard category literal.</param>
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

        /// <summary>
        /// Returns the message to be displayed when a node publishing limit has been reached
        /// </summary>
        /// <returns></returns>
        public string GetMessage()
        {
            //Custom message overrides everything
            if (!string.IsNullOrEmpty(CustomMessage)) {return (CustomMessage);}

            //Return a standard message if this rule is created on the fly based on a special document property value
            if (FromProperty)
            {
                return (string.Format("Node saved but not published. Max allowed child nodes for this specific node: {0}.", MaxNodes.ToString()));
            }
            
            //This is the message that is returned when a rule is in the config file, and no custom message has been defined.
            return string.Format(
                "Node saved but not published. Max allowed nodes {1} directly under {2}: {0}."
                , MaxNodes.ToString()
                , ChildDocType.Equals("*") ? "of any type" : string.Format("of type \"{0}\"", ChildDocType)
                , ParentDocType.Equals("*") ? "any node" : string.Format("nodes of type \"{0}\"", ParentDocType)
                );
        }

        /// <summary>
        /// Returns the literal for the message category
        /// </summary>
        /// <returns></returns>
        public string GetMessageCategory()
        {
            return (string.IsNullOrEmpty(CustomMessageCategory) ? "Publish" : CustomMessageCategory);
        }

        /// <summary>
        /// Returns the warning message to be displayed on publishing a node when a rule is in effect but the limit has not been reached.
        /// </summary>
        /// <param name="currentNodeCount"></param>
        /// <returns></returns>
        public string GetWarningMessage(int currentNodeCount)
        {
            //Custom message overrides everything
            if (!string.IsNullOrEmpty(CustomWarningMessage)) { return (CustomWarningMessage); }

            //Return a standard message if this rule is created on the fly based on a special document property value
            if (FromProperty)
            {
                return (string.Format("Restrictions for this node are in place. You have published {0} out {1} allowed child nodes.", (currentNodeCount+1).ToString(), MaxNodes.ToString()));
            }

            //This is the message that is returned when a rule is in the config file, and no custom message has been defined.
            return string.Format(
                "Restrictions in place. {3} directly under {2}: {1} of {0} allowed."
                , MaxNodes.ToString()
                , (currentNodeCount + 1).ToString()
                , ParentDocType.Equals("*") ? "any node" : string.Format("nodes of type \"{0}\"", ParentDocType)
                , ChildDocType.Equals("*") ? "Any node" : string.Format("Nodes of type \"{0}\"", ChildDocType)
                );
            
        }
        /// <summary>
        /// Returns the literal for the warning message category
        /// </summary>
        /// <returns></returns>
        public string GetWarningMessageCategory()
        {
            return (string.IsNullOrEmpty(CustomWarningMessageCategory) ? "Publish" : CustomWarningMessageCategory);
        }

    }
}
