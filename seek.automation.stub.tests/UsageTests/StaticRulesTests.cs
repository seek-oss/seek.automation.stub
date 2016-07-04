using System;
using System.Collections.Generic;
using System.Net;
using FluentAssertions;
using Newtonsoft.Json;
using RestSharp;
using Xunit;

namespace seek.automation.stub.tests.UsageTests
{
    public class StaticRulesTests
    {
        [Fact]
        public void Validate_When_Response_Is_Set_To_Have_Dynamic_Id_As_Integer()
        {
            var dad = Stub.Create(9000).FromFile("Data/StaticRulesPact.json");

            var client = new RestClient("http://localhost:9000/");
            var request = new RestRequest("/please/give/me/some/money", Method.POST);
            var response = client.Execute(request);

            dad.Dispose();

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var dict = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(response.Content);
            
            int idInt;
            var isIdInt = int.TryParse((string)dict["amount"], out idInt);
            isIdInt.Should().BeTrue("The value of the auto-generated id in the response should be an integer.");
        }


        [Fact]
        public void Validate_When_Response_Is_Set_To_Have_Dynamic_Guid()
        {
            var dad = Stub.Create(9000).FromFile("Data/StaticRulesPact.json");

            var client = new RestClient("http://localhost:9000/");
            var request = new RestRequest("/please/give/me/some/money", Method.POST);
            var response = client.Execute(request);

            dad.Dispose();

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var dict = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(response.Content);
            
            Guid guid;
            var isReciptGuid = Guid.TryParse((string)dict["reciept"], out guid);
            isReciptGuid.Should().BeTrue("The value of the auto-generated reciept in the response should be an GUID.");
        }
    }
}
