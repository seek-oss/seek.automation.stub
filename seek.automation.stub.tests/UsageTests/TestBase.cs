using RestSharp;

namespace seek.automation.stub.tests.UsageTests
{
    public class TestBase
    {
        public RestClient Client;

        public TestBase(string fakeService)
        {
            Client = new RestClient(fakeService);
        }

        public IRestResponse DoHttpPost(string path, string jsonBody = null)
        {
            var request = new RestRequest(path, Method.POST);

            if (!string.IsNullOrEmpty(jsonBody))
            {
                request.RequestFormat = DataFormat.Json;
                request.AddParameter("application/json", jsonBody, ParameterType.RequestBody);
            }

            return Client.Execute(request);
        }
    }
}
