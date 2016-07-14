using System.IO;
using System.Net;
using FluentAssertions;
using seek.automation.stub.tests.Helpers;
using Xunit;

namespace seek.automation.stub.tests.UsageTests
{
    public class FilterTests : TestBase
    {
        private const string FakePactBrokerUrl = "http://localhost:12345/";
        private readonly string _pactAsString;

        public FilterTests() : base("http://localhost:9000/")
        {
            _pactAsString = File.ReadAllText("Data/SimplePact.json");
        }

        [Fact]
        public void Validate_When_Filtered_On_Provider_State()
        {
            var fakePactBroker = new FakePactBroker(FakePactBrokerUrl);
            fakePactBroker.RespondWith(_pactAsString);

            var dad = Stub.Create(9000).FromPactbroker(FakePactBrokerUrl);

            dad.FilterOnProviderState("Dad has enough money and an advice");

            var response = DoHttpPost("/please/give/me/some/money");

            dad.Dispose();
            fakePactBroker.Dispose();

            response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        }

        [Fact]
        public void Validate_When_Filtered_On_Description()
        {
            var fakePactBroker = new FakePactBroker(FakePactBrokerUrl);
            fakePactBroker.RespondWith(_pactAsString);

            var dad = Stub.Create(9000).FromPactbroker(FakePactBrokerUrl);

            dad.FilterOnDescription("a request for money or advice");

            var response = DoHttpPost("/please/give/me/some/money");

            dad.Dispose();
            fakePactBroker.Dispose();

            response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        }

        [Fact]
        public void Validate_When_Filtered_On_Provider_State_And_Description()
        {
            var fakePactBroker = new FakePactBroker(FakePactBrokerUrl);
            fakePactBroker.RespondWith(_pactAsString);

            var dad = Stub.Create(9000).FromPactbroker(FakePactBrokerUrl);

            dad.FilterOnProviderState("Dad has enough money and an advice");
            dad.FilterOnDescription("a request for money or advice");

            var response = DoHttpPost("/please/give/me/some/money");

            dad.Dispose();
            fakePactBroker.Dispose();

            response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        }

        [Fact]
        public void Validate_When_Filters_Are_Cleared()
        {
            var fakePactBroker = new FakePactBroker(FakePactBrokerUrl);
            fakePactBroker.RespondWith(_pactAsString);

            var dad = Stub.Create(9000).FromPactbroker(FakePactBrokerUrl);

            dad.FilterOnProviderState("Dad has enough money and an advice");
            dad.FilterOnDescription("a request for money or advice");
            dad.ClearFilters();

            var response = DoHttpPost("/please/give/me/some/money");

            dad.Dispose();
            fakePactBroker.Dispose();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
