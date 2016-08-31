[![Build status](https://ci.appveyor.com/api/projects/status/skj51qg05wh9md54?svg=true&passingText=project%20-%20OK)](https://ci.appveyor.com/project/b3hdad/seek-automation-stub)
[![Build status](https://ci.appveyor.com/api/projects/status/skj51qg05wh9md54/branch/master?svg=true&passingText=master%20-%20OK)](https://ci.appveyor.com/project/b3hdad/seek-automation-stub/branch/master)


## SEEK Automation Stubbing Library

A [pact](https://github.com/SEEK-Jobs/pact-net) based stubbing library written in C#.

To find this package on Nuget.org, please visit [this page](https://www.nuget.org/packages/seek.automation.stub/).

## Overview

This package is particularly usefull if your team or company is building your product using microservices architecture, using C# language.
It allows you to stub a service by listening on a port and accepting requests, and replying based on the interactions which are specified
in a pact.

If you are intrested in an installable service that can provide similar functionality please see [this page](https://github.com/SEEK-Jobs/SEEK.Automation.Phantom)

The pact can be loaded in, either as a file, JSON string or from pact broker. 

Please see examples below.

## Build

To build the solution:

```> .\build.ps1 -target Build-Solution```

To run the tests:

```> .\build.ps1 -target Run-Unit-Tests```

To package and publish to Nuget:

```> .\build.ps1 -target Publish-Nuget-Package```

When publishing you need to have set the NUGET_ORG_API_KEY environment variable.

## Examples

This one line will create a service listening on port 9000, and loads the specified pact file from hard disk:

```csharp
var fakeService = Stub.Create(9000).FromFile("SimplePact.json");
```

where the SimplePact.json, contains a pact with multiple interactions. Here is a pact file with only one interaction:

```json
{
  "provider": {
    "name": "Dad"
  },
  "consumer": {
    "name": "Child"
  },
  "interactions": [
    {
      "description": "a request for money",
      "provider_state": "Dad has enough money",
      "request": {
        "method": "post",
        "path": "/please/give/me/some/money",
        "headers": {
          "Content-Type": "application/json; charset=utf-8"
        }
      },
      "response": {
        "status": 200
      }
    }
  ]
}
```

If your JSON is a string:

```
var pactAsJsonString = "{\r\n  \"provider\": {\r\n    \"name\": \"Dad\"\r\n  },\r\n  \"consumer\": {\r\n    \"name\": \"Child\"\r\n  },\r\n  \"interactions\": [\r\n    {\r\n      \"description\": \"a request for money\",\r\n      \"provider_state\": \"Dad has enough money\",\r\n   \"request\": {\r\n        \"method\": \"post\",\r\n        \"path\": \"/please/give/me/some/money\",\r\n        \"headers\": {\r\n          \"Content-Type\": \"application/json; charset=utf-8\"\r\n        }\r\n      },\r\n      \"response\": {\r\n        \"status\": 200\r\n      }\r\n    }\r\n  ]\r\n}";
```
You can load it into stub using the FromJson method:

```csharp
var fakeService = Stub.Create(9000).FromJson(pactAsJsonString);
```

Finally, if you want to load the pact from pact broker, use the FromPactbroker method:

```csharp
var fakeService = Stub.Create(9000).FromPactbroker("http://pactbroker/pacts/provider/dad/consumer/child/latest");
```

Please note that the Stub, does a match on the request body by default. The match is a simple string match. If you don't care about the request body, you can ask the Stub to ignore it:

```csharp
var fakeService = Stub.Create(9000).FromFile("SimplePact.json", false);
var fakeService = Stub.Create(9000).FromJson(pactAsJsonString, false);
var fakeService = Stub.Create(9000).FromPactbroker("http://pactbroker/pacts/provider/dad/consumer/child/latest", false);
```

There are also methods available to filter based on the ```provider_state``` or the ```description``` of the interaction. This may be
useful if you have multiple interactions, where the requests are identical and you can not overcome this by having different request bodies: 

```csharp
var fakeService = Stub.Create(9000).FromFile("SimplePact.json");
fakeService.FilterOnProviderState("Dad has enough money and an advice");
fakeService.FilterOnDescription("a request for money or advice");
```

You can also clear the filters at anytime:

```csharp
fakeService.ClearFilters();
```

>Warning: Please do not use the filters if you expect multiple interactions in any run. Since the filter will apply to all of the interactions for that service.

## Authentication Workaround
At times you might encounter scenarios where the request contains authentication tokens. This library automatically
removes the following tokens:

```
oauth_consumer_key
oauth_timestamp
oauth_signature
```

## Performance Feature
This library also provides a feature to allow you to run mini pact based performance tests:

```csharp
[Fact]
public void Mini_Pact_Based_Performance_Test()
{
    using (var testServer = TestServer.Create(app => OwinStartup.Configuration(app, BuildMyServiceUnderTest())))
    {
        var performance = new Performance("http://pactbroker/pacts/provider/dad/consumer/child/latest");

        var pactVerifier = new PactVerifier(() => { }, () => { });

        pactVerifier
            .ProviderState("Dad has enough money")
            .ServiceProvider("Dad", testServer.HttpClient).HonoursPactWith("Child")
            .PactUri(performance.LocalPact);

        performance.Run(() => pactVerifier.Verify("a request for money"), 1000);

        Round(performance.AverageExecutionTime.TotalSeconds).Should().BeLessThan(0.005);
    }
}
```
In the above example, BuildMyServiceUnderTest, is a function that builds the current service, by mocking/stubbing, the dependencies.

Then, everthing is the same as normal pact testing process, except, you do the verifications multiple times by using the
method performance.Run provided by the library.

Please note this is only to detect major changes in the performance of a service. The values also needs to be geared towards the build agent as the devloper's machine could have different performance.

To get a more accurate picture, you can run longer running executions overnight with millions of iterations and make the use
of other methods that the library provides such as:

```
performance.ExecutionTime
performance.MaxExecutionTime
performance.MinExecutionTime
```

## Tips and Tricks

There is a small feature built into the library where if you need to auto-generate some IDs or Guids you can easily do
that. This only applies to the response.

If you have an Id where it needs to be different when the response comes back from the Stub, then you set the value to [INT]. This means that the Stub, will return a random integer, when sending the response back.

Similarly, if you need the stub to return a response where you require a different GUID, then set the value to [GUID].

So if load the following pact into Stub:

```json
{
  "provider": {
    "name": "Dad"
  },
  "consumer": {
    "name": "Child"
  },
  "interactions": [
    {
      "description": "a request for money",
      "provider_state": "Dad has enough money",
      "request": {
        "method": "post",
        "path": "/please/give/me/some/money",
        "headers": {
          "Content-Type": "application/json; charset=utf-8"
        }
      },
      "response": {
        "status": 200,
        "body": {
          "amount": "[INT]",
          "receipt": "[GUID]"
        }
      }
    }
  ]
}
```

Then the response that comes back everytime will have different values for the amount and the receipt:
```json
{
    "status": 200,
    "body": {
      "amount": "19",
      "receipt": "7c4530fd-a689-40db-992b-52fcf4ae983f"
	  }
}
```

## Pact + Stub
When using stub, you can approach pact testing in a slightly different light. 
Currently when generating the pacts as a consumer on a service, we spin up a mock server program it with interactions, exercise those interactions and as the result the pact file gets generated.

Then when we are writing the provider tests for the same service, then we spin up the service mocking some of the dependencies. This means that not the whole code gets exercised depending on level of mocking. Also some bugs that could lie in the serialiser may not get picked up.

Now consider the following service:

```
                       +---------------------+
                       |                     |
                       |                     |
                       |                     |-----------> Provider P1
                       |       Service       |
Consumer C1----------->|        Under        |
			           |       Test(SUT)     |
			           |                     |-----------> Provider P2
			           |                     |
			           |                     |
			           +---------------------+

```

The above service is a provider to C1 and the consumer of services P1 and P2. 
To simplify the process of pact testing you can **generate** two pacts one for P1 and one for P2. This generation process is actually very simple. You can inherit from the class below which uses Pact.Net classes to register interactions and generate your pact files:

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using PactNet.Mocks.MockHttpService.Models;
using PactNet.Models;

namespace My.Test.Name.Space
{
    public class TestSetup : IDisposable
    {
        private readonly string _consumer;
        private readonly string _provider;
        private readonly string _pactDirectory;
        private readonly List<ProviderServiceInteraction> _interactions;

        public TestSetup(string consumer, string provider, string pactDirectory = "pacts")
        {
            _consumer = consumer;
            _provider = provider;
            _pactDirectory = pactDirectory;
            _interactions = new List<ProviderServiceInteraction>();

            if (_pactDirectory == null) _pactDirectory = Directory.GetCurrentDirectory();
            Directory.CreateDirectory(_pactDirectory);
        }

        public void RegisterInteraction(params ProviderServiceInteraction[] interactions)
        {
            _interactions.AddRange(interactions);
        }

        private void GeneratePactFile()
        {
            var pact = new ProviderServicePactFile
            {
                Consumer = new Pacticipant { Name = _consumer },
                Provider = new Pacticipant { Name = _provider },
                Interactions = _interactions
            };

            var pactDetails = (PactDetails)pact;

            var serializedPact = JsonConvert.SerializeObject(pactDetails, new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Converters = new List<JsonConverter> { new StringEnumConverter { CamelCaseText = true } },
                NullValueHandling = NullValueHandling.Ignore
            });

            File.WriteAllText(Path.Combine(_pactDirectory, pactDetails.GeneratePactFileName()), serializedPact, Encoding.UTF8);
        }

        public void Dispose()
        {
            GeneratePactFile();
        }
    }
}
```

To use the above, you can do something similar to below, but of-course you can register multiple interactions, using the RegisterInteraction
method:

```csharp
using System.Collections.Generic;
using System.Net;
using PactNet.Mocks.MockHttpService.Models;
using Xunit;

namespace My.Test.Name.Space
{
    public class ProviderP1Contract : TestSetup
    {
        public ProviderP1Contract() : base("SUT", "P1")
        {
        }

        [Fact]
        public void Create_Pact_For_Sut()
        {
            RegisterInteraction(Default_Sut_Interaction());
        }

        public ProviderServiceInteraction Default_Sut_Interaction()
        {
            return new ProviderServiceInteraction
            {
                ProviderState = "Given there is a user with Id 1",
                Description = "A request for info for user with Id 1",
                Request = new ProviderServiceRequest
                {
                    Method = HttpVerb.Post,
                    Path = "/v1/profile/1",
                    Headers = new Dictionary<string, string>
                    {
                        {"Accept", "application/json"}
                    }
                },
                Response = new ProviderServiceResponse
                {
                    Status = (int)HttpStatusCode.OK,
                    Headers = new Dictionary<string, string>
                    {
                        {"Content-Type", "application/json; charset=utf-8"}
                    },
                    Body = new UserProfile { id = 1,  name = "Joe" }
                }
            };
        }
    }
}
```

So as you see, the above does nothing but only generate the contract using the Pact.net and the SUT types(e.g UserProfile).

Now when you write your provider tests for SUT for C1, if you load the pacts for P1 and P2 using this stubbing library, then as the C1 provider tests run, it will exercise the SUT classes without any mocks and as it was running on a production environment.

## Future Improvements

Some minor point(s) to keep in mind:

* When time permits, the usage tests will be replaced with unit tests. Since usage tests use the Stub and listen on the specified ports, if multiple branches of this projects are built on the same build machine, there could be a conflict. This only applies if you decide to build this on your local build system rather than using the published nuget package.

## Troubleshooting

If you recieve "Access Denied", please check the account that you are using to run your project.
If any other problems please submit an issue or a pull request.

## License Information

This is released under MIT license.

















