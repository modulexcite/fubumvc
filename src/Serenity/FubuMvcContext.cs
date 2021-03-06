using System;
using System.Collections.Generic;
using System.Linq;
using FubuCore;
using FubuMVC.Core.Diagnostics.Runtime;
using HtmlTags;
using StoryTeller;
using StoryTeller.Engine;
using StoryTeller.Results;

namespace Serenity
{
    public class FubuMvcContext : IExecutionContext
    {
        private readonly FubuMvcSystem _system;
        private readonly string _sessionTag = Guid.NewGuid().ToString();

        public FubuMvcContext(FubuMvcSystem system)
        {
            _system = system;
        }

        public void Dispose()
        {
        }

        public IServiceLocator Services
        {
            get { return _system.Application.Services; }
        }

        public T GetService<T>()
        {
            return _system.Application.Services.GetInstance<T>();
        }

        public virtual void AfterExecution(ISpecContext context)
        {
            var reporter = new RequestReporter(_system);
            var requestLogs = Services.GetInstance<IRequestHistoryCache>().RecentReports().Where(x => x.SessionTag == _sessionTag).ToArray();
            reporter.Append(requestLogs);

            context.Reporting.Log(reporter);

            _system.ApplyLogging(context);
        }

        public void BeforeExecution(ISpecContext context)
        {
            Services.GetInstance<IRequestHistoryCache>().CurrentSessionTag = _sessionTag;
           _system.Application.Navigation.Logger = new ContextualNavigationLogger(context);
        }
    }

    public class RequestReporter : Report
    {
        private readonly FubuMvcSystem _system;
        private readonly List<RequestLog> _logs = new List<RequestLog>();

        public RequestReporter(FubuMvcSystem system)
        {
            _system = system;
        }

        public string ShortTitle
        {
            get { return "FubuMVC"; }
        }

        public int Count
        {
            get { return _logs.Count; }
        }


        public string ToHtml()
        {
            var table = new TableTag();
            table.AddClass("table");
            table.AddClass("table-striped");
            table.AddHeaderRow(row =>
            {
                row.Header("Details");
                row.Header("Duration (ms)");
                row.Header("Method");
                row.Header("Endpoint");
                row.Header("Status");
                row.Header("Content Type");
            });

            _logs.Each(log =>
            {
                var url = _system.Application.RootUrl.TrimEnd('/') + "/_fubu/#/fubumvc/request-details/" + log.Id;

                table.AddBodyRow(row =>
                {
                    row.Cell().Add("a").Text("Details").Attr("href", url).Attr("target", "_blank");
                    row.Cell(log.ExecutionTime.ToString()).Attr("align", "right");
                    row.Cell(log.HttpMethod);
                    row.Cell(log.Endpoint);
                    row.Cell(log.StatusCode.ToString());
                    row.Cell(log.ContentType);
                });
            });


            return table.ToString();
        }

        public string Title
        {
            get { return "FubuMVC Requests During the Specification Execution"; }
        }

        public void Append(RequestLog[] requests)
        {
            _logs.AddRange(requests);
        }
    }
}