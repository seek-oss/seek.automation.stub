using Xunit;
using RestSharp;
using FluentAssertions;
using System.Net;

namespace seek.automation.stub.tests.UsageTests
{
    public class AuthenticationTests
    {
        [Fact]
        public void Validate_When_Request_Contains_Authentication_Key()
        {
            var dad = Stub.Create(9000).FromFile("Data/SimplePact.json");

            var client = new RestClient("http://localhost:9000/");
            var request = new RestRequest("/please/give/me/some/money?oauth_consumer_key=please", Method.POST);
            var response = client.Execute(request);

            dad.Dispose();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public void Validate_When_Request_Contains_Authentication_TimeStamp()
        {
            var dad = Stub.Create(9000).FromFile("Data/SimplePact.json");

            var client = new RestClient("http://localhost:9000/");
            var request = new RestRequest("/please/give/me/some/money?oauth_timestamp=now", Method.POST);
            var response = client.Execute(request);

            dad.Dispose();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public void Validate_When_Request_Contains_Authentication_Signature()
        {
            var dad = Stub.Create(9000).FromFile("Data/SimplePact.json");

            var client = new RestClient("http://localhost:9000/");
            var request = new RestRequest("/please/give/me/some/money?oauth_signature=me", Method.POST);
            var response = client.Execute(request);

            dad.Dispose();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public void Validate_When_Request_Contains_Authentication_Tokens()
        {
            var dad = Stub.Create(9000).FromFile("Data/SimplePact.json");

            var client = new RestClient("http://localhost:9000/");
            var request = new RestRequest("/please/give/me/some/money?oauth_consumer_key=please&oauth_timestamp=now&oauth_signature=me&oauth_signature=me", Method.POST);
            var response = client.Execute(request);

            dad.Dispose();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public void Validate_When_Request_Contains_Unexpected_Authentication_Token()
        {
            var dad = Stub.Create(9000).FromFile("Data/SimplePact.json");

            var client = new RestClient("http://localhost:9000/");
            var request = new RestRequest("/please/give/me/some/money?oauth_password=abc", Method.POST);
            var response = client.Execute(request);

            dad.Dispose();

            response.StatusCode.ToString().Should().Be("551");
            response.StatusDescription.Should().Be("Stub on port 9000 says interaction not found. Please verify that the pact associated with this port contains the following request(case insensitive) : Method 'POST', Path '/please/give/me/some/money?oauth_password=abc', Body ''");
        }
    }
}
