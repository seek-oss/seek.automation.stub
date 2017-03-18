using System.Net;
using FluentAssertions;
using Xunit;

namespace seek.automation.stub.tests.UsageTests
{
    public class EchoTests : TestBase
    {
        public EchoTests() : base("http://localhost:9000/")
        {
        }

        [Fact]
        public void Validate_Stub_Can_Echo_Back_Status()
        {
            var dad = Stub.Create(9000).Echo(202);
            
            var response = DoHttpPost("/anything/anyway/", "{ A : \"foo\", B : \"bar\" }");

            dad.Dispose();

            response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        }
    }
}
