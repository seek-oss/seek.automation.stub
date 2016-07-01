using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Serilog;

namespace seek.automation.stub.Helpers
{
    public interface IWebServer : IDisposable
    {
        void Simulate(Func<int, HttpListenerContext, HttpResponseMessage> callbackMethod, int port);

        Func<int, HttpListenerContext, HttpResponseMessage> CallbackMethod { get; set; }
    }

    [SuppressMessage("ReSharper", "UseStringInterpolation")]
    public class WebServer : IWebServer
    {
        private readonly HttpListener _httpListener = new HttpListener();
        private readonly ILogger _logger;

        public Func<int, HttpListenerContext, HttpResponseMessage> CallbackMethod { get; set; }

        public WebServer(ILogger logger)
        {
            _logger = logger;
        }

        public void Simulate(Func<int, HttpListenerContext, HttpResponseMessage> callbackMethod, int port)
        {
            _logger.Information("Starting simulation for port {0}", port);

            var prefix = string.Format("http://*:{0}/", port);

            CallbackMethod = callbackMethod;

            if (CallbackMethod == null)
            {
                throw new ArgumentException("Callback method was not specified");
            }

            if (!HttpListener.IsSupported)
            {
                throw new NotSupportedException("HttpListener is only supported on Windows XP SP2, Server 2003 or later");
            }

            _httpListener.Prefixes.Add(prefix);

            _httpListener.Start();

            Task.Factory.StartNew(() =>
            {
                while (_httpListener.IsListening)
                {
                    var httpListenerContext = _httpListener.GetContext();
                    try
                    {
                        var response = CallbackMethod(port, httpListenerContext);

                        httpListenerContext.Response.StatusCode = (int)response.StatusCode;
                        var content = response.Content != null ? response.Content.ReadAsStringAsync().Result : null;

                        if (!string.IsNullOrEmpty(content))
                        {
                            var buf = Encoding.UTF8.GetBytes(content);
                            httpListenerContext.Response.ContentLength64 = buf.Length;
                            httpListenerContext.Response.OutputStream.Write(buf, 0, buf.Length);
                        }
                    }
                    catch (InteractionNotFoundException ex)
                    {
                        var errorMessage =
                            Message(
                                string.Format("Stub on port {0} says interaction not found. Please verify that the pact associated with this port contains the following request(case insensitive)", port),
                                ex.Message);

                        _logger.Error(ex, errorMessage);

                        httpListenerContext.Response.StatusCode = 551;
                        httpListenerContext.Response.StatusDescription = errorMessage;
                    }
                    catch (Exception ex)
                    {
                        var errorMessage = Message(string.Format("Stub on port {0} says simulation failed. The error is", port), ex.Message);

                        _logger.Error(ex, errorMessage);

                        httpListenerContext.Response.StatusCode = 550;
                        httpListenerContext.Response.StatusDescription = errorMessage;
                    }
                    finally
                    {
                        httpListenerContext.Response.OutputStream.Close();
                    }
                }
            });
        }

        private string Message(string prefixMessage, string exceptionMessage)
        {
            return string.Format("{0} : {1}", prefixMessage, exceptionMessage.Replace(Environment.NewLine, " "));
        }

        public void Dispose()
        {
            _httpListener.Stop();
            _httpListener.Close();
        }
    }
}
