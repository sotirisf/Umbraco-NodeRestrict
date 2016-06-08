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

        public string PropertyAlias { get; private set; }
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
        /// Applies all rules on creation of a node. 
        /// </summary>
        /// <param name="node">The newly created node we need to apply rules for</param>
        public Result Run(IContent node)
        {

            var parent = node.Parent();
            if (parent == null) { return null; }
            if (node.Published) { return null; }

            Result result = null;

            //Swallow any exceptions here. If it's there, it's there. If it's not, don't bother.
            try
            {
                if (
                    parent.HasProperty(PropertyAlias) 
                    && parent.Properties[PropertyAlias]!=null 
                    && (int)parent.Properties[PropertyAlias].Value > 0
                    )
                {
                    Rule customRule = new Rule(parent.ContentType.Alias, "*", (int)parent.Properties[PropertyAlias].Value ,true, true);
                    return CheckRule(customRule, node);
                }
            }
            catch { }

            foreach (Rule rule in _rules)
            {
                result = CheckRule(rule, node);

                if (result!=null)
                {
                    break;
                }
            }
            return (result);
        }

        #endregion

        #region Private Methods

        private Result CheckRule(Rule rule, IContent node) {
            int nodeCount = 0;

            //If maxnodes not at least equal 1 then skip this rule.
            if (rule.MaxNodes <= 0) { return null; }

            bool isMatchParent = node.Parent().ContentType.Alias.Equals(rule.ParentDocType) || rule.ParentDocType.Equals("*");
            bool isMatchChild = rule.ChildDocType.Equals(node.ContentType.Alias) || rule.ChildDocType.Equals("*");

            //If rule doctypes do not match, skip this rule
            if (!isMatchChild || !isMatchParent) { return null; }

            //If parent node already has published child nodes of the same type as the one we are saving
            if (node.Parent().Children().Where(x => x.ContentType.Name == node.ContentType.Name).Any())
            {
                nodeCount = node.Parent().Children().Where(x => x.ContentType.Name == node.ContentType.Name && x.Published).Count();
            }

            return Result.GetResult(nodeCount, rule);


        }

        /// <summary>
        /// Gets rules from /config/Restrictor.config file (if it exists)
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

            PropertyAlias = xmlConfig.SelectNodes("/nodeRestrict")[0].Attributes["propertyAlias"].Value;

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


                    var rule = new Rule(parentDocType, childDocType, maxNodes, false, showWarnings, customMessage, customMessageCategory, customWarningMessage, customWarningMessageCategory);
                    _rules.Add(rule);

                }
            }
        }


       

        #endregion
    }
}