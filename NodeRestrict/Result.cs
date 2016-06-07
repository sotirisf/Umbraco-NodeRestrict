using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Web.Models;

namespace DotSee.NodeRestrict
{
    public class Result
    {
        public int NodeCount { get; private set; }
       public Rule Rule { get; private set; }
        public bool LimitReached { get { return (NodeCount >= Rule.MaxNodes); } }

        private Result(int nodeCount, Rule rule)
        {
            NodeCount = nodeCount;
            Rule = rule;
           
            
        }

        public static Result GetResult(int nodeCount, Rule rule) {
            return new Result(nodeCount,rule);
        }
    }

}
