using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;
using System.ServiceModel;
using System.Text;
using System.Xml;

namespace Node
{
    public class PluginClassNode : IPlugin
    {
                #region Secure/Unsecure Configuration Setup
        private string _secureConfig = null;
        private string _unsecureConfig = null;
        private string webAddress;
        private string fetchXml;

        public PluginClassNode(string unsecureConfig, string secureConfig)
        {
            _secureConfig = secureConfig;
            _unsecureConfig = unsecureConfig;

            if (String.IsNullOrEmpty(_unsecureConfig))
            {
                throw new Exception("must supply configuration data");
            }
            else
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(_unsecureConfig);

                XmlNodeList endpointnodes = doc.DocumentElement.SelectNodes("/config/endpoint");
                if (endpointnodes.Count == 1)
                {
                    webAddress = endpointnodes[0].InnerText;
                }
                else
                {
                    throw new Exception("config data must contain exactly one 'endpoint' element");
                }

                XmlNodeList querynodes = doc.DocumentElement.SelectNodes("/config/query");
                if (querynodes.Count == 1)
                {
                    fetchXml = querynodes[0].InnerText;
                }
                else
                {
                    throw new Exception("config data must contain exactly one 'query' element");
                }

            }
        }

        //private string web;

        /// <summary>
        /// The plug-in constructor.
        /// </summary>
        /// <param name="config">The Web address to access. An empty or null string
        /// defaults to accessing www.bing.com. The Web address can use the HTTP or
        /// HTTPS protocol.</param>
        //public void WebClientPlugin(string config)
        //{
        //    if (String.IsNullOrEmpty(config))
        //    {
        //        web = "http://www.com";
        //    }
        //    else
        //    {
        //        web = config;
        //    }
        //}

                #endregion



                #region CRM Base

        public void Execute(IServiceProvider serviceProvider)
        {
            ITracingService tracer = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = factory.CreateOrganizationService(context.UserId);

            try
            {
                Entity entity = (Entity)context.InputParameters["Target"];

                //TODO: Do stuff
                //retrieve some results using the fetchxml supplied in the configuration
                EntityCollection results = service.RetrieveMultiple(new Microsoft.Xrm.Sdk.Query.FetchExpression(string.Format(fetchXml, entity.Id.ToString())));

                //we should have one and only one result
                if (results.Entities.Count != 1)
                {
                    throw new Exception("query did not return a single result");
                }

                Entity retrieved = results.Entities[0];
                




                #region JSON Processing

                //set up our json writer
                StringBuilder sb = new StringBuilder();
                StringWriter sw = new StringWriter(sb);
                JsonWriter jsonWriter = new JsonTextWriter(sw);

                jsonWriter.Formatting = Newtonsoft.Json.Formatting.Indented;

                jsonWriter.WriteStartObject();

                //loop through the retrieved attributes
                foreach (string attribute in retrieved.Attributes.Keys)
                {
                    //generate different output for different attribute types
                    switch (retrieved[attribute].GetType().ToString())
                    {
                        //if we have a lookup, return the id and the name
                        case "Microsoft.Xrm.Sdk.EntityReference":
                            jsonWriter.WritePropertyName(attribute);
                            jsonWriter.WriteValue(((EntityReference)retrieved[attribute]).Id);
                            jsonWriter.WritePropertyName(attribute + "_name");
                            jsonWriter.WriteValue(((EntityReference)retrieved[attribute]).Name);
                            break;
                        //if we have an optionset value, return the value and the formatted value
                        case "Microsoft.Xrm.Sdk.OptionSetValue":
                            jsonWriter.WritePropertyName(attribute);
                            jsonWriter.WriteValue(((OptionSetValue)retrieved[attribute]).Value);
                            if (retrieved.FormattedValues.Contains(attribute))
                            {
                                jsonWriter.WritePropertyName(attribute + "_formatted");
                                jsonWriter.WriteValue(retrieved.FormattedValues[attribute]);
                            }
                            break;
                        //if we have money, return the value
                        case "Microsoft.Xrm.Sdk.Money":
                            jsonWriter.WritePropertyName(attribute);
                            jsonWriter.WriteValue(((Money)retrieved[attribute]).Value);
                            break;
                        //if we have a datetime, return the value
                        case "System.DateTime":
                            jsonWriter.WritePropertyName(attribute);
                            jsonWriter.WriteValue(retrieved[attribute]);
                            break;
                        //for everything else, return the value and a formatted value if it exists
                        default:
                            jsonWriter.WritePropertyName(attribute);
                            jsonWriter.WriteValue(retrieved[attribute]);
                            if (retrieved.FormattedValues.Contains(attribute))
                            {
                                jsonWriter.WritePropertyName(attribute + "_formatted");
                                jsonWriter.WriteValue(retrieved.FormattedValues[attribute]);
                            }
                            break;
                    }

                }
                //always write out the message name (update, create, etc.), entity name and record id
                jsonWriter.WritePropertyName("operation");
                jsonWriter.WriteValue(context.MessageName);
                jsonWriter.WritePropertyName("entity");
                jsonWriter.WriteValue(retrieved.LogicalName);
                jsonWriter.WritePropertyName("id");
                jsonWriter.WriteValue(retrieved.Id);
                jsonWriter.WriteEndObject();

                //generate the json string
                string jsonMsg = sw.ToString();

                jsonWriter.Close();
                sw.Close();

                //throw new InvalidPluginExecutionException(jsonMsg);

                #endregion


                #region Web Request
                try
                {
                    //create the webrequest object and execute it (and post jsonmsg to it)
                    
                    //for additional information on working with json from dynamics CRM
                    System.Net.WebRequest req = System.Net.WebRequest.Create(webAddress);
                    req.Timeout=10000;
                    //must set the content type for json
                    req.ContentType = "application/json";
                    //must set method to post
                    req.Method = "POST";
                    //create a stream
                    byte[] bytes = System.Text.Encoding.ASCII.GetBytes(jsonMsg.ToString());
                    req.ContentLength = bytes.Length;
                    System.IO.Stream os = req.GetRequestStream();
                    os.Write(bytes, 0, bytes.Length);
                    os.Close();
                    //get the response
                    System.Net.WebResponse resp = req.GetResponse();
                }
                catch (WebException exception)
                {
                    string str = string.Empty;
                    if (exception.Response != null)
                    {
                        using (StreamReader reader =
                        new StreamReader(exception.Response.GetResponseStream()))
                        {
                            str = reader.ReadToEnd();
                        }
                        exception.Response.Close();
                    }
                    if (exception.Status == WebExceptionStatus.Timeout)
                    {
                        throw new InvalidPluginExecutionException(
                        "The timeout elapsed while attempting to issue the request.", exception);
                    }
                    throw new InvalidPluginExecutionException(String.Format(CultureInfo.InvariantCulture,
                    "A Web exception ocurred while attempting to issue the request. {0}: {1}",
                    exception.Message, str), exception);
                }
                catch (Exception e)
                {
                    tracer.Trace("Exception: {0}", e.ToString());
                    throw;
                }

                

        #endregion


            }
            //catch (Exception e)
            //{
            //    throw new InvalidPluginExecutionException(e.Message);
            //}


            catch (Exception e)
            {
                // TODO: Rollback any non-transactional operations
                throw new InvalidPluginExecutionException(e.Message);


                //OrganizationServiceFault fault = new OrganizationServiceFault();
                //fault.ErrorCode = -2147204346; // This will cause the Async Server to retry
                //fault.Message = ex.ToString();
                //var networkException = new FaultException(fault.Message);
                //throw networkException;
            }

                #endregion

            #region Test

            //DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
            //MemoryStream ms = new MemoryStream();
            //ser.WriteObject(ms, retrieved);
            //string jsonString = Encoding.UTF8.GetString(ms.ToArray());
            //ms.Close();
            //try
            //{
            //    tracer.Trace("Downloading the target URI: " + webAddress);

            //    ////try
            //    ////{
            //    ////    // Download the target URI using a Web client. Any .NET class that uses the
            //    ////    // HTTP or HTTPS protocols and a DNS lookup should work.
            //    ////    using (WebClient client = new WebClient())
            //    ////    {

            //    ////        //byte[] responseBytes = client.DownloadData(webAddress);
            //    ////        //string response = Encoding.UTF8.GetString(responseBytes);
            //    ////        //tracer.Trace(response);



            //    ////        //// Optionally specify an encoding for uploading and downloading strings.
            //    ////        //client.Encoding = System.Text.Encoding.UTF8;
            //    ////        //// Upload the data.
            //    ////        //string reply = client.UploadString(webAddress, "ss");

            //    ////        // For demonstration purposes, throw an exception so that the response
            //    ////        // is shown in the trace dialog of the Microsoft Dynamics CRM user interface.




            //    ////        //throw new InvalidPluginExecutionException("WebClientPlugin completed successfully." + reply);
            //    ////    }
            //    ////}

            //    ////catch (WebException exception)
            //    ////{
            //    ////    string str = string.Empty;
            //    ////    if (exception.Response != null)
            //    ////    {
            //    ////        using (StreamReader reader =
            //    ////            new StreamReader(exception.Response.GetResponseStream()))
            //    ////        {
            //    ////            str = reader.ReadToEnd();
            //    ////        }
            //    ////        exception.Response.Close();
            //    ////    }
            //    ////    if (exception.Status == WebExceptionStatus.Timeout)
            //    ////    {
            //    ////        throw new InvalidPluginExecutionException(
            //    ////            "The timeout elapsed while attempting to issue the request.", exception);
            //    ////    }
            //    ////    throw new InvalidPluginExecutionException(String.Format(CultureInfo.InvariantCulture,
            //    ////        "A Web exception occurred while attempting to issue the request. {0}: {1}",
            //    ////        exception.Message, str), exception);
            //    ////}









            //}
            //catch (Exception e)
            //{
            //    tracer.Trace("Exception: {0}", e.ToString());
            //    throw;
            //}




            ////create the webrequest object and execute it (and post jsonmsg to it)
            ////see http://www.alexanderdevelopment.net/post/2013/04/22/Postingprocessing-JSON-in-a-CRM-2011-custom-work... 
            ////for additional information on working with json from dynamics CRM
            //System.Net.WebRequest req = System.Net.WebRequest.Create(webAddress);
            //req.Timeout = 100000;
            ////must set the content type for json
            //req.ContentType = "application/json";

            ////must set method to post
            //req.Method = "POST";

            ////create a stream
            //byte[] bytes = System.Text.Encoding.ASCII.GetBytes(jsonMsg.ToString());
            //req.ContentLength = bytes.Length;
            //System.IO.Stream os = req.GetRequestStream();
            //os.Write(bytes, 0, bytes.Length);
            //os.Close();


            ////Testing
            ////byte[] bytes = System.Text.Encoding.ASCII.GetBytes("hi");
            ////req.ContentLength = bytes.Length;
            ////System.IO.Stream os = req.GetRequestStream();
            ////os.Write(bytes, 0, bytes.Length);
            ////os.Close();



            ////get the response
            //System.Net.WebResponse resp = req.GetResponse();

            ////throw new InvalidPluginExecutionException(resp.Headers.ToString()+" "+resp.ContentType.ToString());
            #endregion
        }
    }
}
