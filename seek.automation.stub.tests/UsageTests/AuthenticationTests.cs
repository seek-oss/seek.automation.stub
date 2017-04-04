using System.Net;
using FluentAssertions;
using Xunit;

namespace seek.automation.stub.tests.UsageTests
{
    public class AuthenticationTests : TestBase
    {
        public AuthenticationTests() : base("http://localhost:9000/")
        {
        }

        [Fact]
        public void Validate_When_Request_Contains_Authentication_Key()
        {
            var dad = Stub.Create(9000).FromFile("Data/SimplePact.json");

            var response = DoHttpPost("/please/give/me/some/money?oauth_consumer_key=please");

            dad.Dispose();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public void Validate_When_Request_Contains_Authentication_TimeStamp()
        {
            var dad = Stub.Create(9000).FromFile("Data/SimplePact.json");
            
            var response = DoHttpPost("/please/give/me/some/money?oauth_timestamp=now");

            dad.Dispose();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public void Validate_When_Request_Contains_Authentication_Signature()
        {
            var dad = Stub.Create(9000).FromFile("Data/SimplePact.json");
            
            var response = DoHttpPost("/please/give/me/some/money?oauth_signature=me");

            dad.Dispose();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public void Validate_When_Request_Contains_Authentication_Tokens()
        {
            var dad = Stub.Create(9000).FromFile("Data/SimplePact.json");
            
            var response = DoHttpPost("/please/give/me/some/money?oauth_consumer_key=please&oauth_timestamp=now&oauth_signature=me&oauth_signature=me");

            dad.Dispose();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public void Validate_When_Request_Contains_Unexpected_Authentication_Token()
        {
            var dad = Stub.Create(9000).FromFile("Data/SimplePact.json");
            
            var response = DoHttpPost("/please/give/me/some/money?oauth_password=abc");

            dad.Dispose();

            response.StatusCode.ToString().Should().Be("551");
            response.StatusDescription.Should().Be("Stub on port 9000 says interaction not found. Please verify that the pact associated with this port contains the following request(case insensitive) : Method 'POST', Path '/please/give/me/some/money?oauth_password=abc', Body ''. If you have specified filters please also check them.");
        }
    }
}
