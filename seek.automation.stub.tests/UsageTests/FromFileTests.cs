using System.Net;
using FluentAssertions;
using RestSharp;
using Xunit;

namespace seek.automation.stub.tests.UsageTests
{
    public class FromFileTests
    {
        [Fact]
        public void Validate_When_Request_Is_Matched()
        {
            var dad = Stub.Create(9000).FromFile("Data/SimplePact.json");
            
            var client = new RestClient("http://localhost:9000/");
            var request = new RestRequest("/please/give/me/some/money", Method.POST);
            var response = client.Execute(request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            dad.Dispose();
        }

        [Fact]
        public void Validate_When_Request_Is_Not_Matched()
        {
            var dad = Stub.Create(9000).FromFile("Data/SimplePact.json");

            var client = new RestClient("http://localhost:9000/");
            var request = new RestRequest("/please/give/me/some/food", Method.POST);
            var response = client.Execute(request);
            
            response.StatusCode.ToString().Should().Be("551");
            response.StatusDescription.Should().Be("Stub on port 9000 says interaction not found. Please verify that the pact associated with this port contains the following request(case insensitive) : Method 'POST', Path '/please/give/me/some/food', Body ''");

            dad.Dispose();
        }
    }
}
