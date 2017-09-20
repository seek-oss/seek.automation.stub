using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using PactNet.Mocks.MockHttpService.Models;
using PactNet.Models;
using seek.automation.stub.Pact;

namespace seek.automation.stub.Helpers
{
    [ExcludeFromCodeCoverage]
    public class PactTransformer<T> where T : class
    {
        private readonly string _consumerName;
        private readonly string _providerName;
        private Func<string, T, T> _transformer = (path, request) => request;
        private readonly IList<ProviderServiceInteraction> _interactions = new List<ProviderServiceInteraction>();

        public PactTransformer(string consumerName, string providerName)
        {
            _consumerName = consumerName;
            _providerName = providerName;
        }

        // Add an optional transformer for the request otherwise use the passthrough default.
        // .WithTransformer((originalPath, request) => ....) 
        // Where originalPath is the current provider path for this service and request is the request body and payload.
        public PactTransformer<T> WithTransform(Func<string, T, T> transformer)
        {
            _transformer = transformer;

            return this;
        }

        //no transform clears prior transform and passes through the same request.
        public PactTransformer<T> WithNoTransform()
        {
            return WithTransform((path, request) => request);
        }

        public IEnumerable<ProviderServiceInteraction> Interactions
        {
            get { return _interactions; }
        }

        // Transform a pactfile to pass-through use in Stub to the redirected path
        public PactTransformer<T> RedirectFor(string pactFilename, string redirectPath)
        {
            var payload = Helper.GetPactViaBroker(pactFilename);

            var pactFile = JsonConvert.DeserializeObject<ProviderServicePactFile>(payload);

            foreach (var interaction in pactFile.Interactions)
            {
                var body = _transformer(interaction.Request.Path?.ToString(), interaction.Request.Body.ToObject<T>());

                interaction.Request.Path = redirectPath;

                interaction.Request.Body = body;

                _interactions.Add(interaction);
            }

            return this;
        }

        public void SavePactFile(string filepath)
        {
            var pactDetails = CreateProviderServicePactFile();

            File.WriteAllText(Path.Combine(filepath, pactDetails.GeneratePactFileName()), ToPactAsJson(), Encoding.UTF8);
        }

        public string ToPactAsJson()
        {
            var pactDetails = CreateProviderServicePactFile();

            return JsonConvert.SerializeObject(pactDetails, new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Converters = new List<JsonConverter> { new StringEnumConverter { CamelCaseText = true } },
                NullValueHandling = NullValueHandling.Ignore
            });
        }

        private ProviderServicePactFile CreateProviderServicePactFile()
        {
            var pactDetails = new ProviderServicePactFile
            {
                Consumer = new Pacticipant { Name = _consumerName },
                Provider = new Pacticipant { Name = _providerName },
                Interactions = _interactions
            };
            return pactDetails;
        }
    }
}