using System.IO;
using Xunit;
using FluentAssertions;
using System.Net;

namespace seek.automation.stub.tests.UsageTests
{
    public class FromJsonTests : TestBase
    {
        private readonly string _pactAsString;
        
        public FromJsonTests() : base("http://localhost:9000/")
        {
            _pactAsString = File.ReadAllText("Data/SimplePact.json");
        }

        [Fact]
        public void Validate_When_Request_Is_Matched()
        {
            var dad = Stub.Create(9000).FromJson(_pactAsString);
            
            var response = DoHttpPost("/please/give/me/some/money");

            dad.Dispose();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public void Validate_When_Request_Is_Not_Matched()
        {
            var dad = Stub.Create(9000).FromJson(_pactAsString);
            
            var response = DoHttpPost("/please/give/me/some/food");

            dad.Dispose();

            response.StatusCode.ToString().Should().Be("551");
            response.StatusDescription.Should().Be("Stub on port 9000 says interaction not found. Please verify that the pact associated with this port contains the following request(case insensitive) : Method 'POST', Path '/please/give/me/some/food', Body ''");
        }
    }
}
