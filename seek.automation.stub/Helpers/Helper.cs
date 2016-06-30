﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using Nancy.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PactNet;
using PactNet.Mocks.MockHttpService.Models;

namespace seek.automation.stub
{
    [SuppressMessage("ReSharper", "UseStringInterpolation")]
    public class Helper
    {
        public static HttpResponseMessage PactRegistration(string payload, HttpListenerContext listenerContext, bool matchBody)
        {
            var pactFile = JsonConvert.DeserializeObject<ProviderServicePactFile>(payload);
            var pb = new PactBuilder();
            pb.ServiceConsumer(pactFile.Consumer.Name);

            var responseToCurrentRequest = GetResponseForRequestFromPact(pactFile, listenerContext.Request, matchBody);

            return responseToCurrentRequest;
        }

        public static string GetPactViaBroker(string pactBrokerUrl)
        {
            var myRequest = (HttpWebRequest)WebRequest.Create(pactBrokerUrl);
            myRequest.Method = "GET";
            var myResponse = myRequest.GetResponse();

            // ReSharper disable once AssignNullToNotNullAttribute
            var sr = new StreamReader(myResponse.GetResponseStream(), System.Text.Encoding.UTF8);
            var result = sr.ReadToEnd();
            sr.Close();
            myResponse.Close();

            return result;
        }

        private static HttpResponseMessage GetResponseForRequestFromPact(ProviderServicePactFile pactFile, HttpListenerRequest request, bool matchBody)
        {
            var requestBody = GetRequestPostData(request);

            foreach (var providerServiceInteraction in pactFile.Interactions)
            {
                var requestPathMatches = RequestPathMatches(request, providerServiceInteraction);

                var requestMethodMatches = providerServiceInteraction.Request.Method.ToString().Equals(request.HttpMethod, StringComparison.OrdinalIgnoreCase);

                var pactRequestBody = providerServiceInteraction.Request.Body == null ? string.Empty : providerServiceInteraction.Request.Body.ToString();

                var requestBodyMatches = !matchBody || pactRequestBody.Equals(requestBody, StringComparison.OrdinalIgnoreCase);
                if (requestPathMatches && requestMethodMatches && requestBodyMatches)
                {
                    var response = new HttpResponseMessage
                    {
                        StatusCode = (HttpStatusCode)providerServiceInteraction.Response.Status
                    };

                    var content = providerServiceInteraction.Response.Body == null ? string.Empty : providerServiceInteraction.Response.Body.ToString();
                    response.Content = new StringContent(ApplyStaticRules(content));

                    return response;
                }
            }

            throw new InteractionNotFoundException(string.Format("Method '{0}', Path '{1}', Body '{2}'", request.HttpMethod, request.Url.PathAndQuery, requestBody));
        }

        private static bool RequestPathMatches(HttpListenerRequest request, ProviderServiceInteraction providerServiceInteraction)
        {
            // Compare base url
            bool baseUrlMatches = string.Compare(request.Url.LocalPath, providerServiceInteraction.Request.Path, StringComparison.OrdinalIgnoreCase) == 0;

            if (baseUrlMatches)
            {
                // Compare query string
                if (string.IsNullOrEmpty(request.Url.Query))
                {
                    return string.IsNullOrEmpty(providerServiceInteraction.Request.Query);
                }

                var currentPactQuery = HttpUtility.ParseQueryString(providerServiceInteraction.Request.Query ?? "");
                var currentQuery = HttpUtility.ParseQueryString(request.Url.Query);
                var isCurrentPactQueryTheSameAsCurrentRequestQuery = currentPactQuery.FilterCollection().CollectionEquals(currentQuery.FilterCollection());
                return isCurrentPactQueryTheSameAsCurrentRequestQuery;
            }
            return false;
        }

        public static string GetRequestPostData(HttpListenerRequest request)
        {
            if (!request.HasEntityBody)
            {
                return string.Empty;
            }

            using (var bodyInputStream = request.InputStream)
            {
                using (var reader = new StreamReader(bodyInputStream, request.ContentEncoding))
                {
                    var body = reader.ReadToEnd();
                    return request.ContentType == "application/json; charset=utf-8" ? JObject.Parse(body).ToString() : body;
                }
            }
        }

        private static string ApplyStaticRules(string str)
        {
            var random = new Random();

            var updatedString = str.Replace("[GUID]", Guid.NewGuid().ToString());
            updatedString = updatedString.Replace("[INT]", random.Next().ToString(CultureInfo.InvariantCulture));
            return updatedString;
        }
    }
}