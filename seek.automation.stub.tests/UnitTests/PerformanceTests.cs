using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using seek.automation.stub.tests.Helpers;
using FluentAssertions;
using Xunit;

namespace seek.automation.stub.tests.UnitTests
{
    [SuppressMessage("ReSharper", "ConvertPropertyToExpressionBody")]
    public class StartWatch : IStopWatch
    {
        public Stopwatch StopWatch;
        public TimeSpan TimeSpan;

        public StartWatch(){StopWatch = new Stopwatch();}
        public void Start(){StopWatch.Start();}
        public void Stop(){StopWatch.Stop();}
        public void Reset(){StopWatch.Reset();}
        
        public TimeSpan Elapsed
        {
            get { return TimeSpan; }
            set { TimeSpan = value; }
        }
    }

    public class PerformanceTests
    {
        private const string FakePactBrokerUrl = "http://localhost:12345/";
        private const string PactAsJson = "{\r\n  \"provider\": {\r\n    \"name\": \"Dad\"\r\n  },\r\n  \"consumer\": {\r\n    \"name\": \"Child\"\r\n  },\r\n  \"interactions\": [\r\n    {\r\n      \"description\": \"a request for money\",\r\n      \"provider_state\": \"Dad has enough money\",\r\n   \"request\": {\r\n        \"method\": \"post\",\r\n        \"path\": \"/please/give/me/some/money\",\r\n        \"headers\": {\r\n          \"Content-Type\": \"application/json; charset=utf-8\"\r\n        }\r\n      },\r\n      \"response\": {\r\n        \"status\": 200\r\n      }\r\n    }\r\n  ]\r\n}";
        
        [Fact]
        public void Validate_Execution_Time()
        {
            var fakePactBroker = new FakePactBroker(FakePactBrokerUrl);
            fakePactBroker.RespondWith(PactAsJson);

            var fakeStopWatch = new StartWatch {Elapsed = new TimeSpan(0, 0, 0, 1234)};
            var fakeLapStopWatch = new StartWatch {Elapsed = new TimeSpan(0, 0, 0, 1234)};
            var performance = new Performance(FakePactBrokerUrl, fakeStopWatch, fakeLapStopWatch);

            performance.Run(() => { }, 10);

            fakePactBroker.Dispose();

            Performance.Round(performance.ExecutionTime.TotalSeconds).Should().Be(1234);
        }

        [Fact]
        public void Validate_Average_Execution_Time()
        {
            var fakePactBroker = new FakePactBroker(FakePactBrokerUrl);
            fakePactBroker.RespondWith(PactAsJson);

            var fakeStopWatch = new StartWatch { Elapsed = new TimeSpan(0, 0, 0, 1234) };
            var fakeLapStopWatch = new StartWatch { Elapsed = new TimeSpan(0, 0, 0, 1234) };
            var performance = new Performance(FakePactBrokerUrl, fakeStopWatch, fakeLapStopWatch);

            performance.Run(() => { }, 10);

            fakePactBroker.Dispose();

            Performance.Round(performance.AverageExecutionTime.TotalSeconds).Should().Be(123.4);
        }

        [Fact]
        public void Validate_Pact_File_Is_Downloaded_From_Pact_Broker_To_Local_Machine()
        {
            var fakePactBroker = new FakePactBroker(FakePactBrokerUrl);
            fakePactBroker.RespondWith(PactAsJson);
            
            var performance = new Performance(FakePactBrokerUrl);

            fakePactBroker.Dispose();

            Assert.True(File.Exists(performance.LocalPact), "Failed to download the pact to local machine.");
        }

        [Fact]
        public void Validate_Pact_File_Is_Copied_If_Local_Pact_Is_Specified()
        {
            var performance = new Performance("Data/SimplePact.json");

            Assert.True(File.Exists(performance.LocalPact), "Failed to download the pact to local machine.");
        }

        [Fact]
        public void Validate_Exception_When_Pact_Broker_Is_Not_Found()
        {
            var ex = Assert.Throws<WebException>(() => new Performance("http://somewhere.here.there"));

            ex.Message.Should().Be("The remote name could not be resolved: 'somewhere.here.there'");
        }

        [Fact]
        public void Validate_Exception_When_Pact_File_Is_Not_Found()
        {
            var ex = Assert.Throws<FileNotFoundException>(() => new Performance("NoFile.json"));

            ex.Message.Should().Be("Could not find file 'NoFile.json'.");
        }
    }
}
