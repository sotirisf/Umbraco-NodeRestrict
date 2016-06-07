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
            string createdDocType = node.ContentType.Alias;

            bool hasChildren = node.Children().Any();

            //bool limitReached = false;
            Result result = null;
            int nodeCount = 0;

            foreach (Rule rule in _rules)
            {
                if (!rule.ChildDocType.Equals(createdDocType) || node.Published==true) { continue; }

                if (
                    rule.IncludeChildren==false 
                    && node.Parent().Children().Where(x => x.ContentType.Name == node.ContentType.Name).Any()
                   )
                {
                    nodeCount = node.Parent().Children().Where(x => x.ContentType.Name == node.ContentType.Name && x.Published).Count();
                }
                else if (
                        rule.IncludeChildren == true
                        && node.Parent().Descendants().Where(x => x.ContentType.Name == node.ContentType.Name).Any()
                        )
                {
                    nodeCount = node.Parent().Descendants().Where(x => x.ContentType.Name == node.ContentType.Name && x.Published).Count();
                }

                result = Result.GetResult(nodeCount, rule);

                if (nodeCount >= rule.MaxNodes)
                {
                    
                    break;
                }
            }
            return (result);
        }

        #endregion

        #region Private Methods

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

            foreach (XmlNode xmlConfigEntry in xmlConfig.SelectNodes("/nodeRestrict/rule"))
            {
                if (xmlConfigEntry.NodeType == XmlNodeType.Element)
                {
                    string parentDocType = xmlConfigEntry.Attributes["parentDocType"].Value;
                    string childDocType = xmlConfigEntry.Attributes["childDocType"].Value;
                    int maxNodes=-1;
                    int.TryParse(xmlConfigEntry.Attributes["maxNodes"].Value, out maxNodes);
                    bool includeChildren = false;
                    includeChildren= bool.TryParse(xmlConfigEntry.Attributes["includeChildren"].Value, out includeChildren);
                    bool showWarnings = false;
                    showWarnings = bool.TryParse(xmlConfigEntry.Attributes["showWarnings"].Value, out showWarnings);
                    string customMessage = xmlConfigEntry.Attributes["customMessage"].Value;
                    string customMessageCategory = xmlConfigEntry.Attributes["customMessageCategory"].Value;

                    var rule = new Rule(parentDocType, childDocType, maxNodes, includeChildren, showWarnings, customMessage, customMessageCategory);
                    _rules.Add(rule);

                }
            }
        }


       

        #endregion
    }
}