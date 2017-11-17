using System;
using System.IO;
using System.Net;
using System.Text;

namespace Pathfinder.Util.Network
{
    public class WebClient : System.Net.WebClient
    {
        public new virtual WebRequest GetWebRequest(Uri address) => base.GetWebRequest(address);
    }

    public class RestManager
    {
        public string Fetch(string url,
                            string method = WebRequestMethods.Http.Get,
                            string toSend = "",
                            string apiKey = "",
                            bool forJson = true)
        {
            DateTime startedOnUtc = DateTime.UtcNow;
            string responseData = null;
            string responseHeaders = null;
            int responseStatusCode = 0;
            var headerValue = "application/" + (forJson ? "json" : "xml");

            try
            {
                var webClient = new WebClient();
                webClient.Proxy = null;
                webClient.Headers.Add(HttpRequestHeader.Accept, headerValue);

                // You can use BASIC authorization like so (you must implement your own Recurly.ApiKeyBase64String): 
                webClient.Headers.Add(HttpRequestHeader.Authorization, "Basic " + apiKey);

                if (!String.IsNullOrWhiteSpace(toSend))
                    webClient.Headers.Add(HttpRequestHeader.ContentType, headerValue + "; charset=utf-8");
                var webRequest = webClient.GetWebRequest(new Uri(url));
                webRequest.Method = method;

                if (!String.IsNullOrWhiteSpace(toSend))
                {
                    using (var writer = new StreamWriter(webRequest.GetRequestStream(), Encoding.UTF8))
                        writer.WriteLine(toSend);
                }
                else
                    if (method == WebRequestMethods.Http.Put)
                        webRequest.ContentLength = 0; // ContentLength == 0 is required for PUT requests

                using (var response = webRequest.GetResponse())
                    ProcessResponse(response, out responseData, out responseHeaders, out responseStatusCode);
            }
            catch (WebException ex)
            {
                using (ex.Response)
                    ProcessResponse(ex.Response, out responseData, out responseHeaders, out responseStatusCode);

                responseData = "<!-- " + ex.Message + " -->\r\n\r\n" + responseData;
            }
            finally
            {
                /* YOU CAN (you should) LOG ALL YOUR COMMUNICATION WITH SERVER HERE like this:

                  var log = Log.CreateCommunicationLogObject();

                  log.StartedOnUtc = startedOnUtc;
                  log.FinishedOnUtc = DateTime.UtcNow;
                  log.RequestMethod = method;
                  log.RequestData = String.IsNullOrWhiteSpace(xmlToSend) ? null : xmlToSend;
                  log.RequestUrl = url;
                  log.ResponseData = responseData;
                  log.ResponseStatusCode = responseStatusCode;
                  log.ResponseHeaders = responseHeaders;

                  Log.Save();
                */
            }

            if (responseStatusCode >= 200 && responseStatusCode < 300)
                // All 2xx HTTP statuses are OK
                return responseData;

            return String.Empty;
        }

        private static void ProcessResponse(WebResponse response,
                                            out string recurlyResponse,
                                            out string responseHeaders,
                                            out int responseStatusCode)
        {
            recurlyResponse = responseHeaders = String.Empty;

            using (var responseStream = response.GetResponseStream())
                using (var sr = new StreamReader(responseStream, Encoding.UTF8))
                    recurlyResponse = sr.ReadToEnd();

            responseHeaders = response.Headers.ToString();
            responseStatusCode = (int)((response as HttpWebResponse)?.StatusCode ?? 0);
        }
    }
}