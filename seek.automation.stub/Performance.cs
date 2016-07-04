using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;

namespace seek.automation.stub
{
    public interface IStopWatch
    {
        void Start();
        void Stop();
        void Reset();
        TimeSpan Elapsed { get; set; }
    }

    [SuppressMessage("ReSharper", "ConvertPropertyToExpressionBody")]
    [ExcludeFromCodeCoverage]
    public class CustomStopWatch : IStopWatch
    {
        public Stopwatch StopWatch;

        public CustomStopWatch(){StopWatch = new Stopwatch();}
        public void Start(){StopWatch.Start();}
        public void Stop(){StopWatch.Stop();}
        public void Reset(){StopWatch.Reset();}

        [ExcludeFromCodeCoverage]
        public TimeSpan Elapsed
        {
            get { return StopWatch.Elapsed; }
            set { throw new NotImplementedException(); }
        }
    }

    [SuppressMessage("ReSharper", "UseStringInterpolation")]
    [SuppressMessage("ReSharper", "UseNullPropagation")]
    public class Performance
    {
        public TimeSpan ExecutionTime { get; private set; }
        public TimeSpan AverageExecutionTime { get; private set; }
        public TimeSpan MaxExecutionTime { get; private set; }
        public TimeSpan MinExecutionTime { get; private set; }
        public IStopWatch StopWatch { get; set; }
        public IStopWatch LapStopWatch { get; set; }
        public string LocalPact { get; set; }

        public Performance(string pactUri, IStopWatch stopWatch = null, IStopWatch lapStopWatch = null)
        {
            MaxExecutionTime = new TimeSpan(long.MinValue);
            MinExecutionTime = new TimeSpan(long.MaxValue);

            StopWatch = stopWatch ?? new CustomStopWatch();
            LapStopWatch = lapStopWatch ?? new CustomStopWatch();

            LocalPact = string.Format("PerformancePactFile-{0}.json", DateTime.Now.ToString("yyyyMMdd-hhmmssfffffff"));

            SavePactLocally(pactUri, LocalPact);
        }

        public void Run(Action action, int iterations)
        {
            LapStopWatch = LapStopWatch ?? new CustomStopWatch();

            action();

            StopWatch.Start();
            {
                for (var i = 0; i < iterations; i++)
                {
                    LapStopWatch.Start();
                    action();
                    LapStopWatch.Stop();

                    if (LapStopWatch.Elapsed > MaxExecutionTime) MaxExecutionTime = LapStopWatch.Elapsed;
                    if (LapStopWatch.Elapsed < MinExecutionTime) MinExecutionTime = LapStopWatch.Elapsed;

                    LapStopWatch.Reset();
                }
            }

            StopWatch.Stop();

            AverageExecutionTime = new TimeSpan(StopWatch.Elapsed.Ticks / iterations);

            ExecutionTime = StopWatch.Elapsed;
        }

        private void SavePactLocally(string pactUri, string localPactFileName)
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
                    }
                }
                return;
            }
            
            File.Copy(pactUri, localPactFileName);
        }

        public static double Round(double number)
        {
            return Math.Round(number, 3, MidpointRounding.ToEven);
        }
    }
}
