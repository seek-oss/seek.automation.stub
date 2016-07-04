using System.IO;
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

            dad.Dispose();

            response.StatusCode.Should().Be(HttpStatusCode.OK);

        }

        [Fact]
        public void Validate_When_Response_Has_Body()
        {
            var dad = Stub.Create(9000).FromFile("Data/PactWithRespBody.json");

            var client = new RestClient("http://localhost:9000/");
            var request = new RestRequest("/please/give/me/some/money", Method.POST);
            var response = client.Execute(request);

            dad.Dispose();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Should().BeEquivalentTo("{\r\n  \"Money\": \"10\"\r\n}");
        }

        [Fact]
        public void Validate_When_Request_Is_Not_Matched()
        {
            var dad = Stub.Create(9000).FromFile("Data/SimplePact.json");

            var client = new RestClient("http://localhost:9000/");
            var request = new RestRequest("/please/give/me/some/food", Method.POST);
            var response = client.Execute(request);

            dad.Dispose();

            response.StatusCode.ToString().Should().Be("551");
            response.StatusDescription.Should().Be("Stub on port 9000 says interaction not found. Please verify that the pact associated with this port contains the following request(case insensitive) : Method 'POST', Path '/please/give/me/some/food', Body ''");
        }

        [Fact]
        public void Validate_When_Request_Has_Body_Default_Set_To_Match_Body()
        {
            var dad = Stub.Create(9000).FromFile("Data/PactWithReqBody.json");

            var client = new RestClient("http://localhost:9000/");
            var request = new RestRequest("/please/give/me/some/money", Method.POST);
            request.AddParameter("application/json", "{\r\n  \"name\": \"Joe\"\r\n}", ParameterType.RequestBody);
            var response = client.Execute(request);

            dad.Dispose();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public void Validate_When_Request_Has_Body_But_Is_Ignored()
        {
            var dad = Stub.Create(9000).FromFile("Data/PactWithReqBody.json", matchBody: false);

            var client = new RestClient("http://localhost:9000/");
            var request = new RestRequest("/please/give/me/some/money", Method.POST);
            request.AddParameter("application/json", "{\"name\": \"Joe\"}", ParameterType.RequestBody);
            var response = client.Execute(request);

            dad.Dispose();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public void Validate_When_Request_Has_Body_But_Is_Not_Matched()
        {
            var dad = Stub.Create(9000).FromFile("Data/PactWithReqBody.json", matchBody: true);

            var client = new RestClient("http://localhost:9000/");
            var request = new RestRequest("/please/give/me/some/money", Method.POST);
            request.AddParameter("application/json", "{\"name\": \"Jack\"}", ParameterType.RequestBody);
            var response = client.Execute(request);

            dad.Dispose();

            response.StatusCode.ToString().Should().Be("551");
            response.StatusDescription.Should().Be("Stub on port 9000 says interaction not found. Please verify that the pact associated with this port contains the following request(case insensitive) : Method 'POST', Path '/please/give/me/some/money', Body '{\"name\": \"Jack\"}'");
        }

        [Fact]
        public void Validate_When_File_Is_Not_Found()
        {
            var ex = Assert.Throws<FileNotFoundException>(() => Stub.Create(9000).FromFile("NoFile.json"));

            ex.Message.Should().Be("Unable to find the specified file.");
        }
    }
}
