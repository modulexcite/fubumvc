using System.Linq;
using FubuCore.Dates;
using FubuMVC.Core.Http;
using FubuMVC.Core.Http.Headers;
using FubuMVC.Core.Registration.Nodes;

namespace FubuMVC.Core.Diagnostics.Runtime.Tracing
{
    public class RequestLogBuilder : IRequestLogBuilder
    {
        private readonly ISystemTime _systemTime;
        private readonly IHttpRequest _request;
        private readonly ICurrentChain _currentChain;
        private readonly DiagnosticsSettings _settings;

        public RequestLogBuilder(ISystemTime systemTime, IHttpRequest request, ICurrentChain currentChain,
            DiagnosticsSettings settings)
        {
            _systemTime = systemTime;
            _request = request;
            _currentChain = currentChain;
            _settings = settings;
        }

        public RequestLog BuildForCurrentRequest()
        {
            var log = new RequestLog
            {
                Hash = _currentChain.Current.GetHashCode(),
                Time = _systemTime.UtcNow(),
            };

            if (_currentChain.IsInPartial())
            {
                log.HttpMethod = "Partial";

                if (_currentChain.Current.InputType() != null)
                {
                    log.Endpoint = _currentChain.Current.InputType().FullName;
                }
                else
                {
                    log.Endpoint = _currentChain.Current.Title();
                }
            }
            else
            {
                if (_settings.TraceLevel == TraceLevel.Verbose)
                {
                    heavyTrace(log);
                }

                if (_currentChain.Current is RoutedChain)
                {
                    log.HttpMethod = _request.HttpMethod();
                    log.Endpoint = _request.RelativeUrl();
                }
                else if (_currentChain.Current.InputType() != null)
                {
                    log.Endpoint = _currentChain.Current.InputType().FullName;
                    log.HttpMethod = "n/a";
                }
                else
                {
                    log.Endpoint = _currentChain.Current.Title();
                    log.HttpMethod = "n/a";
                }

            }


            return log;
        }

        private void heavyTrace(RequestLog log)
        {
            log.RequestHeaders = _request.AllHeaderKeys().SelectMany(x => _request.GetHeader(x).Select(_ => new Header(x, _))).ToArray();

            log.FormData = _request.Form;
            log.QueryString = _request.QueryString;
        }
    }
}