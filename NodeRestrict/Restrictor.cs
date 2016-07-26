using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using umbraco;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using System.Web.Hosting;
using System.IO;

namespace DotSee.NodeRestrict
{
    /// <summary>
    /// Creates new nodes under a newly created node, according to a set of rules
    /// </summary>
    public sealed class Restrictor 
    {

        #region Private Members
        /// <summary>
        /// Lazy singleton instance member
        /// </summary>
        private static readonly Lazy<Restrictor> _instance = new Lazy<Restrictor>(()=>new Restrictor());

        /// <summary>
        /// The list of rule objects
        /// </summary>
        private List<Rule> _rules;
      
        #endregion

        #region Constructors

        /// <summary>
        /// Returns a (singleton) Restrictor instance
        /// </summary>
        public static Restrictor Instance { get { return _instance.Value; } }


        /// <summary>
        /// Private constructor for Singleton
        /// </summary>
        private Restrictor()
        {
            _rules = new List<Rule>();

            ///Get rules from the config file. Any rules programmatically declared later on will be added too.
            GetRulesFromConfigFile();
        }

        #endregion

        #region Public Properties
        
        /// <summary>
        /// Holds the property alias for the "special" property that can be added to nodes to indicate max number of children
        /// </summary>
        public string PropertyAlias { get; private set; }

        /// <summary>
        /// True if showWarnings attribute is true in config file for warnings when applying limits based on a document property
        /// </summary>
        public bool ShowWarningsForProperty { get; private set; }
        
        #endregion

        #region Public Methods

        /// <summary>
        /// Registers a new rule object 
        /// </summary>
        /// <param name="rule">The rule object</param>
        public void RegisterRule(Rule rule)
        {
            _rules.Add(rule);
        }

        /// <summary>
        /// Applies all rules on publishing a node. 
        /// </summary>
        /// <param name="node">The newly created node we need to apply rules for</param>
        public Result Run(IContent node)
        {
            //Get the parent node.
            var parent = node.Parent();

            //If we are publishing a top-level node, skip the whole process.
            if (parent == null) { return null; }
            
            //If the node is already published (and is just being republished) skip the whole process.
            if (node.Published) { return null; }

            Result result = null;

            //Check if the document's parent has the (optional) "special" property that defines the 
            //maximum number of children. If it does, then this overrides any other rules in effect.
            //Swallow any exceptions here. If it's there, it's there. If it's not, don't bother.
            try
            {
                if (
                    parent.HasProperty(PropertyAlias) 
                    && parent.Properties[PropertyAlias]!=null 
                    && (int)parent.Properties[PropertyAlias].Value > 0
                    )
                {
                    //Create a rule on the fly and apply it for all children of the parent node.
                    Rule customRule = new Rule(parent.ContentType.Alias, "*", (int)parent.Properties[PropertyAlias].Value, true, ShowWarningsForProperty);
                    return CheckRule(customRule, node);
                }
            }
            catch { }

            //If this part is reached, then we haven't found a "special" property at the parent node
            //and we are going to check the rules loaded from the config file.
            foreach (Rule rule in _rules)
            {
                //Check if rule applies
                result = CheckRule(rule, node);

                //Stop at the first rule that applies. 
                if (result != null) { break; }
            }

            return (result);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Checks if a given rule applies to a given node
        /// </summary>
        /// <param name="rule">The rule</param>
        /// <param name="node">The node to check against the rule</param>
        /// <returns>Null if the rule does not apply to the node, or a Result object if it does.</returns>
        private Result CheckRule(Rule rule, IContent node) {

            int nodeCount = 0;

            //If maxnodes not at least equal 1 then skip this rule.
            if (rule.MaxNodes <= 0) { return null; }

            //See if doctypes for parent and child node match our current scenario
            bool isMatchParent = node.Parent().ContentType.Alias.Equals(rule.ParentDocType) || rule.ParentDocType.Equals("*");
            bool isMatchChild = rule.ChildDocType.Equals(node.ContentType.Alias) || rule.ChildDocType.Equals("*");

            //If rule doctypes do not match, skip this rule
            if (!isMatchChild || !isMatchParent) { return null; }

            if (rule.ChildDocType.Equals("*"))
            {
                //Check if parent node already contains published child nodes 
                if (node.Parent().Children().Where(x => x.Published).Any())
                {
                    //Get a count of the nodes (all nodes)
                    nodeCount = node.Parent().Children().Where(x => x.Published).Count();
                }
            }
            else
            {
                //Check if parent node already contains published child nodes of the same type as the one we are saving
                if (node.Parent().Children().Where(x => x.ContentType.Name == node.ContentType.Name).Any())
                {
                    //Get a count of the nodes
                    nodeCount = node.Parent().Children().Where(x => x.ContentType.Name == node.ContentType.Name && x.Published).Count();
                }

            }

            return Result.GetResult(nodeCount, rule);
        }

        /// <summary>
        /// Gets rules from /config/nodeRestrict.config file (if it exists)
        /// </summary>
        private void GetRulesFromConfigFile()
        {
            XmlDocument xmlConfig = new XmlDocument();

            try
            {
                xmlConfig.Load(HostingEnvironment.MapPath(GlobalSettings.Path + "/../config/nodeRestrict.config"));
            }
            catch (FileNotFoundException ex) { return; }
            catch (Exception ex)
            {
                Umbraco.Core.Logging.LogHelper.Error(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, "There was a problem loading Restrictor configuration from the config file", ex);
                return;
            }

            //Get the "special" property alias that is going to be optionally used in documents to define max number of children
            PropertyAlias = xmlConfig.SelectNodes("/nodeRestrict")[0].Attributes["propertyAlias"].Value;
            
            try
            {
                ShowWarningsForProperty = bool.Parse(xmlConfig.SelectNodes("/nodeRestrict")[0].Attributes["showWarnings"].Value);
            }
            catch { ShowWarningsForProperty = false; }

            foreach (XmlNode xmlConfigEntry in xmlConfig.SelectNodes("/nodeRestrict/rule"))
            {
                if (xmlConfigEntry.NodeType == XmlNodeType.Element)
                {
                    string parentDocType = xmlConfigEntry.Attributes["parentDocType"].Value;
                    string childDocType = xmlConfigEntry.Attributes["childDocType"].Value;
                    int maxNodes=-1;
                    int.TryParse(xmlConfigEntry.Attributes["maxNodes"].Value, out maxNodes);

                    bool showWarnings = false;
                    try
                    {
                        showWarnings = bool.Parse(xmlConfigEntry.Attributes["showWarnings"].Value);
                    } catch { }

                    string customMessage = xmlConfigEntry.Attributes["customMessage"].Value;
                    string customMessageCategory = xmlConfigEntry.Attributes["customMessageCategory"].Value;
                    string customWarningMessage = xmlConfigEntry.Attributes["customWarningMessage"].Value;
                    string customWarningMessageCategory = xmlConfigEntry.Attributes["customWarningMessageCategory"].Value;

                    //Create the rule and add it to the list
                    Rule rule = new Rule(parentDocType, childDocType, maxNodes, false, showWarnings, customMessage, customMessageCategory, customWarningMessage, customWarningMessageCategory);
                    _rules.Add(rule);

                }
            }
        }

        #endregion
    }
}