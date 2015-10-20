using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;

namespace ConvertLeadNoContact
{
    public class PluginStopConvertContact : IPlugin
    {
        #region Secure/Unsecure Configuration Setup
        private string _secureConfig = null;
        private string _unsecureConfig = null;

        public PluginStopConvertContact(string unsecureConfig, string secureConfig)
        {
            _secureConfig = secureConfig;
            _unsecureConfig = unsecureConfig;
        }
        #endregion
        public void Execute(IServiceProvider serviceProvider)
        {
            ITracingService tracer = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = factory.CreateOrganizationService(context.UserId);

            try
            {


                EntityReference leadid = (EntityReference)context.InputParameters["LeadId"];
                ColumnSet attributes2 = new ColumnSet(new string[] { "subject"});
                Entity lead = service.Retrieve(leadid.LogicalName, leadid.Id, attributes2);
                //throw new InvalidPluginExecutionException(lead.Attributes["subject"].ToString());

                context.InputParameters["CreateContact"] = false;

                //if (context.InputParameters.Contains("Target"))
                //{
                //    Entity entity = (Entity)context.InputParameters["Target"];
                //}

                //else
                //{
                //    throw new InvalidPluginExecutionException("Hi");
                //}

                //TODO: Do stuff

                //if (entity.Attributes["mobilephone"].ToString()=="" || entity.Attributes["mobilephone"]==null)
                //{}
                    //context.InputParameters["CreateContact"] = false;
                   
                

            }
            catch (Exception e)
            {
                throw new InvalidPluginExecutionException(e.Message);
            }
        }
    }
}
