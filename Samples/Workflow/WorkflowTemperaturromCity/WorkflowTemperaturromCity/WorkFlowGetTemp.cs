using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;
using System.IO;
using System.Xml;

namespace WorkflowTemperaturromCity
{
    public class WorkFlowGetTemp : CodeActivity
    {
        [Input("City")]
        [RequiredArgument]
        public InArgument<string> City { get; set; }

        [Input("Country")]
        [RequiredArgument]
        public InArgument<string> Country { get; set; }

        //[Output("Temp_F")]
        //public OutArgument<string> Temp_F { get; set; }

        [Output("Temp_C")]
        public OutArgument<string> Temp_C { get; set; }


        protected override void Execute(CodeActivityContext executionContext)
        {
            ITracingService tracer = executionContext.GetExtension<ITracingService>();
            IWorkflowContext context = executionContext.GetExtension<IWorkflowContext>();
            IOrganizationServiceFactory serviceFactory = executionContext.GetExtension<IOrganizationServiceFactory>();
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            try
            {
                //Entity entity = (Entity)context.InputParameters["Target"];

                //TODO: Do stuff

                string city = City.Get(executionContext);
                string country = Country.Get(executionContext);
                Weather.GlobalWeather Client = new Weather.GlobalWeather();
                string Get = Client.GetWeather(city,country);







               

                String xmlString = Get;

                // Create an XmlReader
                using (XmlReader reader = XmlReader.Create(new StringReader(xmlString)))
                {
                    //reader.ReadStartElement("CurrentWeather");

                    while (reader.Read())
                    {
                        switch (reader.Name.ToString())
                        {
                            case "Temperature":
                                string Temp=reader.ReadString();
                                Temp_C.Set(executionContext, Temp);
                                break;

                        }


                        
                        
                    }
                   
                    
                  
                }

               





            }
            catch (Exception e)
            {
                throw new InvalidPluginExecutionException(e.Message);
            }
        }
    }
}
