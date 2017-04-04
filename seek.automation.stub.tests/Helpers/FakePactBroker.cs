using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace seek.automation.stub.tests.Helpers
{
    public class FakePactBroker
    {
        readonly string _fakePactBrokerUrl;
        readonly HttpListener _listener = new HttpListener();

        public FakePactBroker(string listenOn)
        {
            _fakePactBrokerUrl = listenOn;
        }

        public void RespondWith(string json)
        {
            _listener.Prefixes.Add(_fakePactBrokerUrl);
            _listener.Start();
            
            Task.Factory.StartNew(() =>
            {
                while (_listener.IsListening)
                {
                    var context = _listener.GetContext();
                    try
                    {
                        var response = context.Response;

                        var buffer = Encoding.UTF8.GetBytes(json);
                        response.ContentLength64 = buffer.Length;
                        var output = response.OutputStream;
                        output.Write(buffer, 0, buffer.Length);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Fake pact broker failed with exception: {0}", ex.Message);
                    }
                    finally
                    {
                        context.Response.OutputStream.Close();
                    }
                }
            });
        }

        public void Dispose()
        {
            _listener.Stop();
        }
    }
}
