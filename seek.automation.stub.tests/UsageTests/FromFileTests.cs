using Xunit;
using RestSharp;
using FluentAssertions;
using System.Net;

namespace seek.automation.stub.tests
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
    }
}
