using Xunit;
using RestSharp;
using FluentAssertions;
using System.Net;

namespace seek.automation.stub.tests.UsageTests
{
    public class FromJsonTests
    {
        private string _pactAsJson = "{\r\n  \"provider\": {\r\n    \"name\": \"Dad\"\r\n  },\r\n  \"consumer\": {\r\n    \"name\": \"Child\"\r\n  },\r\n  \"interactions\": [\r\n    {\r\n      \"description\": \"a request for money\",\r\n      \"provider_state\": \"Dad has enough money\",\r\n   \"request\": {\r\n        \"method\": \"post\",\r\n        \"path\": \"/please/give/me/some/money\",\r\n        \"headers\": {\r\n          \"Content-Type\": \"application/json; charset=utf-8\"\r\n        }\r\n      },\r\n      \"response\": {\r\n        \"status\": 200\r\n      }\r\n    }\r\n  ]\r\n}";

        [Fact]
        public void Validate_When_Request_Is_Matched()
        {
            var dad = Stub.Create(9000).FromJson(_pactAsJson);

            var client = new RestClient("http://localhost:9000/");
            var request = new RestRequest("/please/give/me/some/money", Method.POST);
            var response = client.Execute(request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            dad.Dispose();
        }

        [Fact]
        public void Validate_When_Request_Is_Not_Matched()
        {
            var dad = Stub.Create(9000).FromJson(_pactAsJson);

            var client = new RestClient("http://localhost:9000/");
            var request = new RestRequest("/please/give/me/some/food", Method.POST);
            var response = client.Execute(request);

            response.StatusCode.ToString().Should().Be("551");
            response.StatusDescription.Should().Be("Stub on port 9000 says interaction not found. Please verify that the pact associated with this port contains the following request(case insensitive) : Method 'POST', Path '/please/give/me/some/food', Body ''");

            dad.Dispose();
        }
    }
}
