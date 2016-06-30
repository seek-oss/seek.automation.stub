using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;

namespace seek.automation.stub
{
    [SuppressMessage("ReSharper", "UseStringInterpolation")]
    public class Performance
    {
        public TimeSpan ExecutionTime { get; private set; }
        public TimeSpan AverageExecutionTime { get; private set; }
        public TimeSpan MaxExecutionTime { get; private set; }
        public TimeSpan MinExecutionTime { get; private set; }
        public Stopwatch StopWatch { get; set; }
        public string LocalPact { get; set; }

        public Performance(string pactUri)
        {
            MaxExecutionTime = new TimeSpan(long.MinValue);
            MinExecutionTime = new TimeSpan(long.MaxValue);

            StopWatch = new Stopwatch();
            LocalPact = string.Format("PerformancePactFile-{0}.json", DateTime.Now.ToString("yyyyMMdd-hhmmssff"));

            LocalisePact(pactUri, LocalPact);
        }

        public void Run(Action action, int iterations)
        {
            var lapStopWatch = new Stopwatch();

            action();

            StopWatch.Start();
            {
                for (var i = 0; i < iterations; i++)
                {
                    lapStopWatch.Start();
                    action();
                    lapStopWatch.Stop();

                    if (lapStopWatch.Elapsed > MaxExecutionTime) MaxExecutionTime = lapStopWatch.Elapsed;
                    if (lapStopWatch.Elapsed < MinExecutionTime) MinExecutionTime = lapStopWatch.Elapsed;

                    lapStopWatch.Reset();
                }
            }

            StopWatch.Stop();

            AverageExecutionTime = new TimeSpan(StopWatch.Elapsed.Ticks / iterations);

            ExecutionTime = StopWatch.Elapsed;
        }

        private void LocalisePact(string pactUri, string localPactFileName)
        {
            if (pactUri.StartsWith("http"))
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(pactUri);
                httpWebRequest.Method = WebRequestMethods.Http.Get;
                httpWebRequest.Accept = "text/json";
                httpWebRequest = (HttpWebRequest)WebRequest.Create(pactUri);

                using (var outputFile = File.OpenWrite(localPactFileName))
                using (var inputStream = httpWebRequest.GetResponse().GetResponseStream())
                {
                    if (inputStream != null)
                    {
                        inputStream.CopyTo(outputFile);
                        return;
                    }

                    throw new Exception("Failed to read to the Pact file!");
                }
            }

            File.Copy(pactUri, localPactFileName);

            if (!File.Exists(localPactFileName)) throw new FileNotFoundException("Failed to read the pact file {0}", localPactFileName);
        }

        public static double Round(double number)
        {
            return Math.Round(number, 3, MidpointRounding.ToEven);
        }
    }
}
