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

            //Get an instance (it's a singleton) that will also load rules from configuration file.
            Restrictor au = Restrictor.Instance;

            ContentService.Publishing += ContentService_Publishing;
        }

        private void ContentService_Publishing(Umbraco.Core.Publishing.IPublishingStrategy sender, PublishEventArgs<IContent> args)
        {
            Result result = null;

            foreach (IContent node in args.PublishedEntities)
            {
                //This is where the magic happens. Unicorns. Free burgers. 
                result = Restrictor.Instance.Run(node);
            }

            //No rule applied, as you were.
            if (result==null) { return; }

            //If a result has come back, see if limit has been reached or not.
            if (result.LimitReached)
            {
                //Show limit reached message to warn user that he/she has no hope of ever publishing another node.
                args.CancelOperation(new EventMessage(result.Rule.GetMessageCategory(), result.Rule.GetMessage(), EventMessageType.Error));
            }
            else if (result.Rule.ShowWarnings)
            {
                //Show warning message to let the user know how many nodes can still be published.
                args.Messages.Add(new EventMessage(result.Rule.GetWarningMessageCategory(), result.Rule.GetWarningMessage(result.NodeCount), EventMessageType.Warning));
            }
        }
    }
}
