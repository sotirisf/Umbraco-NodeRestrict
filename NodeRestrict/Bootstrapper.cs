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
               
                args.CancelOperation(new EventMessage(result.Rule.GetMessageCategory(), result.Rule.GetMessage(), EventMessageType.Error));

            }
            else if (result.Rule.ShowWarnings) {
               

                args.Messages.Add(new EventMessage(result.Rule.GetWarningMessageCategory(), result.Rule.GetWarningMessage(result.NodeCount), EventMessageType.Warning));
            }
        }


    }
}
