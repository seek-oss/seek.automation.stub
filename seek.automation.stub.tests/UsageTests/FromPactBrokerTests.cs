using System.IO;
using System.Net;
using Xunit;
using FluentAssertions;
using seek.automation.stub.tests.Helpers;

namespace seek.automation.stub.tests.UsageTests
{
    public class FromPactBrokerTests : TestBase
    {
        private const string FakePactBrokerUrl = "http://localhost:12345/";
        private readonly string _pactAsString;
        
        public FromPactBrokerTests() : base("http://localhost:9000/")
        {
            _pactAsString = File.ReadAllText("Data/SimplePact.json");
        }

        [Fact]
        public void Validate_When_Request_Is_Matched()
        {
            var fakePactBroker = new FakePactBroker(FakePactBrokerUrl);
            fakePactBroker.RespondWith(_pactAsString);

            var dad = Stub.Create(9000).FromPactbroker(FakePactBrokerUrl);
            
            var response = DoHttpPost("/please/give/me/some/money");

            dad.Dispose();
            fakePactBroker.Dispose();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public void Validate_When_Request_Is_Not_Matched()
        {
            var fakePactBroker = new FakePactBroker(FakePactBrokerUrl);
            fakePactBroker.RespondWith(_pactAsString);

            var dad = Stub.Create(9000).FromPactbroker(FakePactBrokerUrl);
            
            var response = DoHttpPost("/please/give/me/some/food");

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
