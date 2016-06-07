using Umbraco.Core;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace DotSee.NodeRestrict
{
    public class Bootstrapper : ApplicationEventHandler
    {

        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            base.ApplicationStarted(umbracoApplication, applicationContext);

            Restrictor au = Restrictor.Instance;

            ContentService.Publishing += ContentService_Publishing;

        }

        private void ContentService_Publishing(Umbraco.Core.Publishing.IPublishingStrategy sender, PublishEventArgs<IContent> args)
        {
            Result result = null;
            foreach (IContent node in args.PublishedEntities)
            {

                result = Restrictor.Instance.Run(node);
            }
            if (result==null) { return; }

            if (result.LimitReached)
            {
                string category = "";
                string message = "";
                if (!string.IsNullOrEmpty(result.Rule.CustomMessage))
                {
                    category = result.Rule.CustomMessageCategory;
                    message = result.Rule.CustomMessage;
                }
                else
                {
                    category = "Limit reached - node not published";
                    message = (result.Rule.IncludeChildren) ?
                        string.Format("Max allowed \"{2}\" nodes under \"{1}\" and its child nodes: {0}.", result.Rule.MaxNodes.ToString(), result.Rule.ParentDocType, result.Rule.ChildDocType)
                        :
                        string.Format("Max allowed \"{1}\" nodes directly under \"{2}\": {0}.", result.Rule.MaxNodes.ToString(), result.Rule.ChildDocType, result.Rule.ParentDocType)
                        ;
                }
                args.CancelOperation(new EventMessage(category, message, EventMessageType.Error));

            }
            else if (result.Rule.ShowWarnings) {
                
                string category = "Caution";
                
                string message = (result.Rule.IncludeChildren) ?
                    string.Format("Restrictions in place. \"{3}\" nodes under \"{1}\" and its child nodes: {2} of {0} allowed.", result.Rule.MaxNodes.ToString(), result.Rule.ParentDocType, (result.NodeCount+1).ToString(), result.Rule.ChildDocType)
                    :
                    string.Format("Restrictions in place. \"{3}\" nodes directly under \"{1}\": {2} of {0} allowed.", result.Rule.MaxNodes.ToString(), result.Rule.ParentDocType, (result.NodeCount+1).ToString(), result.Rule.ChildDocType)
                    ;
                args.Messages.Add(new EventMessage(category, message, EventMessageType.Warning));
            }
        }


    }
}
