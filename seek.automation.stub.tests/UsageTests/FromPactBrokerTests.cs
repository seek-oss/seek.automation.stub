using System.IO;
using System.Net;
using Xunit;
using RestSharp;
using FluentAssertions;
using seek.automation.stub.tests.Helpers;

namespace seek.automation.stub.tests.UsageTests
{
    public class FromPactBrokerTests
    {
        private const string FakePactBrokerUrl = "http://localhost:12345/";
        private const string MyServiceUrl = "http://localhost:9000/";
        private const string PactAsJson = "{\r\n  \"provider\": {\r\n    \"name\": \"Dad\"\r\n  },\r\n  \"consumer\": {\r\n    \"name\": \"Child\"\r\n  },\r\n  \"interactions\": [\r\n    {\r\n      \"description\": \"a request for money\",\r\n      \"provider_state\": \"Dad has enough money\",\r\n   \"request\": {\r\n        \"method\": \"post\",\r\n        \"path\": \"/please/give/me/some/money\",\r\n        \"headers\": {\r\n          \"Content-Type\": \"application/json; charset=utf-8\"\r\n        }\r\n      },\r\n      \"response\": {\r\n        \"status\": 200\r\n      }\r\n    }\r\n  ]\r\n}";

        [Fact]
        public void Validate_When_Request_Is_Matched()
        {
            var fakePactBroker = new FakePactBroker(FakePactBrokerUrl);
            fakePactBroker.RespondWith(PactAsJson);

            var dad = Stub.Create(9000).FromPactbroker(FakePactBrokerUrl);
            
            var client = new RestClient(MyServiceUrl);
            var request = new RestRequest("/please/give/me/some/money", Method.POST);
            var response = client.Execute(request);

            dad.Dispose();
            fakePactBroker.Dispose();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public void Validate_When_Request_Is_Not_Matched()
        {
            var fakePactBroker = new FakePactBroker(FakePactBrokerUrl);
            fakePactBroker.RespondWith(PactAsJson);

            var dad = Stub.Create(9000).FromPactbroker(FakePactBrokerUrl);

            var client = new RestClient(MyServiceUrl);
            var request = new RestRequest("/please/give/me/some/food", Method.POST);
            var response = client.Execute(request);

            dad.Dispose();
            fakePactBroker.Dispose();

            response.StatusCode.ToString().Should().Be("551");
            response.StatusDescription.Should().Be("Stub on port 9000 says interaction not found. Please verify that the pact associated with this port contains the following request(case insensitive) : Method 'POST', Path '/please/give/me/some/food', Body ''");
        }

        [Fact]
        public void Validate_When_Pact_Is_Not_Valid()
        {
            var exception = Assert.Throws<InvalidDataException>(() => Stub.Create(9000).FromJson("<Invalid Json"));
            exception.Message.Should().Be("The pact file is not a valid JSON document.");
        }
    }
}
