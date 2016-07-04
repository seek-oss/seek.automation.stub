using Xunit;
using RestSharp;
using FluentAssertions;
using System.Net;

namespace seek.automation.stub.tests.UsageTests
{
    public class EchoTests
    {
        [Fact]
        public void Validate_Stub_Can_Echo_Back_Status()
        {
            var dad = Stub.Create(9000).Echo(202);

            var client = new RestClient("http://localhost:9000/");
            var request = new RestRequest("/anything/anyway/", Method.POST) {RequestFormat = DataFormat.Json};
            request.AddBody(new { A = "foo", B = "bar" });
            var response = client.Execute(request);

            dad.Dispose();

            response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        }
    }
}
