## SEEK Automation Stubbing Library

A pact based stubbing library written in C#.

To find this package on Nuget.org, please visit https://www.nuget.org/packages/seek.automation.stub/

## Overview

This package is particularly usefull if your team or company is building your product using microservice architecture, using C# language.
It allows you to stub a service by listening on a port and accepting requests, and replying based on the interactions which are specified
in a pact.

The pact can be loaded in, either as a file, JSON string or from pact broker.

Please see examples below.

## Example

This one line will create a service listening on port 9000, and loads the specified pact file from hard disk:

```var fakeService = Stub.Create(9000).FromFile("SimplePact.json");```

where the SimplePact.json, contains a pact with multiple interactions. Here is a pact file with only one interaction:

```
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

In order to load the pact as JSON string you can do this:

```
var pactAsJsonString = "{\r\n  \"provider\": {\r\n    \"name\": \"Dad\"\r\n  },\r\n  \"consumer\": {\r\n    \"name\": \"Child\"\r\n  },\r\n  \"interactions\": [\r\n    {\r\n      \"description\": \"a request for money\",\r\n      \"provider_state\": \"Dad has enough money\",\r\n   \"request\": {\r\n        \"method\": \"post\",\r\n        \"path\": \"/please/give/me/some/money\",\r\n        \"headers\": {\r\n          \"Content-Type\": \"application/json; charset=utf-8\"\r\n        }\r\n      },\r\n      \"response\": {\r\n        \"status\": 200\r\n      }\r\n    }\r\n  ]\r\n}";

var fakeService = Stub.Create(9000).FromJson(pactAsJsonString);
```

Finally, if you want to load the pact from pact broker, use the FromPactbroker method:

```
var fakeService = Stub.Create(9000).FromPactbroker("http://pactbroker/pacts/provider/dad/consumer/child/latest");
```

## Authentication Issues
At times you might encounter scenarios where the request contains authentication tokens. This library automatically
removes the following tokens:

```
oauth_consumer_key
oauth_timestamp
oauth_signature
```

## Performance Feature
This library also provides a feature to allow you to run mini pact based performance based tests:

```
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

## Notes

I have added usage tests however, as time permits more unit tests will be added.














