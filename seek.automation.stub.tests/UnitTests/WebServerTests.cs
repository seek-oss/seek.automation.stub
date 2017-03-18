using System;
using FluentAssertions;
using NSubstitute;
using RestSharp;
using seek.automation.stub.Helpers;
using Serilog;
using Xunit;

namespace seek.automation.stub.tests.UnitTests
{
    public class WebServerTests
    {
        [Fact]
        public void Validate_When_Callback_Method_Is_Not_Specified()
        {
            var logger = Substitute.For<ILogger>();

            var webServer = new WebServer(logger);
            
            var ex = Assert.Throws<ArgumentException>(() => webServer.Simulate(null, 2000));

            ex.Message.Should().BeEquivalentTo("Callback method was not specified");
        }

        [Fact]
        public void Validate_When_Callback_Method_Throws()
        {
            var logger = Substitute.For<ILogger>();

            var webServer = new WebServer(logger);
            webServer.Simulate((port, listenerContext) => { throw new Exception("Because I can"); }, 12345);

            var client = new RestClient("http://localhost:12345");
            var request = new RestRequest("/please/give/me/some/food", Method.POST);
            var response = client.Execute(request);

            response.StatusCode.ToString().Should().Be("550");
            response.StatusDescription.Should().Be("Stub on port 12345 says simulation failed. The error is : Because I can");

            webServer.Dispose();
        }

        [Fact]
        public void Validate_When_Callback_Method_Throws_InteractionNotFoundException()
        {
            var logger = Substitute.For<ILogger>();

            var webServer = new WebServer(logger);
            webServer.Simulate((port, listenerContext) => { throw new InteractionNotFoundException("Because I can't"); }, 12345);

            var client = new RestClient("http://localhost:12345");
            var request = new RestRequest("/please/give/me/some/food", Method.POST);
            var response = client.Execute(request);

            response.StatusCode.ToString().Should().Be("551");
            response.StatusDescription.Should().Be("Stub on port 12345 says interaction not found. Please verify that the pact associated with this port contains the following request(case insensitive) : Because I can't");

            webServer.Dispose();
        }
    }
}
