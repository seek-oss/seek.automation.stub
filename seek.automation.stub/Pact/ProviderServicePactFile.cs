using System.Collections.Generic;
using Newtonsoft.Json;
using PactNet.Mocks.MockHttpService.Models;

namespace seek.automation.stub.Pact
{
    public class ProviderServicePactFile : PactFile
    {
        [JsonProperty(PropertyName = "interactions")]
        public IEnumerable<ProviderServiceInteraction> Interactions { get; set; }
    }
}