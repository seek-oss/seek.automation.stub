using System;
using System.Net;
using System.Threading.Tasks;

namespace seek.automation.stub.tests.Helpers
{
    public class FakePactBroker
    {
        HttpListener _listener = new HttpListener();

        public void RespondWith(string json)
        {
            var prefix = string.Format("http://*:{0}/", 12345);
            _listener.Prefixes.Add(prefix);
            _listener.Start();
            
            Task.Factory.StartNew(() =>
            {
                while (_listener.IsListening)
                {
                    var context = _listener.GetContext();
                    try
                    {
                        var fakeResponse = context.Response;

                        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(json);
                        fakeResponse.ContentLength64 = buffer.Length;
                        System.IO.Stream output = fakeResponse.OutputStream;
                        output.Write(buffer, 0, buffer.Length);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(string.Format("Fake pact broker failed with exception: {0}", ex.Message));
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
