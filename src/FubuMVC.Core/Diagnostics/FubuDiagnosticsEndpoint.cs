﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using FubuMVC.Core.Assets;
using FubuMVC.Core.Assets.JavascriptRouting;
using FubuMVC.Core.Diagnostics.Assets;
using FubuMVC.Core.Http;
using FubuMVC.Core.Registration;
using HtmlTags;

namespace FubuMVC.Core.Diagnostics
{

    [Tag("Diagnostics")]
    public class FubuDiagnosticsEndpoint
    {
        private readonly IAssetTagBuilder _tags;
        private readonly IHttpResponse _response;
        private readonly IDiagnosticAssets _assets;
        private readonly JavascriptRouteWriter _routeWriter;
        private readonly DiagnosticJavascriptRoutes _routes;
        private readonly IHttpRequest _request;

        private static readonly string[] _styles = new[] {"bootstrap.min.css", "master.css", "bootstrap.overrides.css"};
        private static readonly string[] _scripts = new[] { "jquery.min.js", "typeahead.bundle.min.js", "root.js" };

        public FubuDiagnosticsEndpoint(
            IAssetTagBuilder tags, 
            IHttpResponse response,
            IDiagnosticAssets assets, 
            JavascriptRouteWriter routeWriter, 
            DiagnosticJavascriptRoutes routes,
            IHttpRequest request)
        {
            _tags = tags;
            _response = response;
            _assets = assets;
            _routeWriter = routeWriter;
            _routes = routes;
            _request = request;

            
        }

        public HtmlDocument get__fubu()
        {
            var document = new HtmlDocument
            {
                Title = "FubuMVC Diagnostics"
            };

            writeStyles(document);

            var foot = new HtmlTag("foot");
            document.Body.Next = foot;
            writeScripts(foot);




            return document;
        }

        private void writeScripts(HtmlTag foot)
        {
            // Do this regardless
            foot.Append(_assets.For("FubuDiagnostics.js").ToScriptTag(_request));

            var routeData = _routeWriter.WriteJavascriptRoutes("FubuDiagnostics.routes", _routes);
            foot.Append(routeData);

            var extensionFiles = _assets.JavascriptFiles().Where(x => x.AssemblyName != "FubuMVC.Core");

            if (FubuMode.Mode() == "diagnostics")
            {
                var names = _scripts.Union(extensionFiles.Select(x => x.Name));
                var links = _tags.BuildScriptTags(names.Select(x => "fubu-diagnostics/" + x));
                links.Each(x => foot.Append(x));
            }
            else
            {
                _scripts.Each(name =>
                {
                    var file = _assets.For(name);
                    foot.Append(file.ToScriptTag(_request));
                });

                extensionFiles.Each(file => foot.Append(file.ToScriptTag(_request)));
            }
        }

        private void writeStyles(HtmlDocument document)
        {
            _styles.Each(name =>
            {
                var file = _assets.For(name);
                document.Head.Append(file.ToStyleTag(_request));
            });
        }

        public void get__fubu_asset_Version_Name(DiagnosticAssetRequest request)
        {
            var file = _assets.For(request.Name);
            if (file == null)
            {
                _response.StatusCode = (int) HttpStatusCode.NotFound;
                return;
            }

            file.Write(_response);
        }

        public void get__fubu_icon(Standin standin)
        {
            var file = _assets.For("fubumvc.png");
            file.Write(_response);
        }
    }

    public class Standin { }

    public class DiagnosticAssetRequest
    {
        public string Version { get; set; }
        public string Name { get; set; }
    }

    public class DiagnosticJavascriptRoutes : JavascriptRouter
    {
        public DiagnosticJavascriptRoutes(BehaviorGraph graph)
        {
            graph.Behaviors.OfType<DiagnosticChain>().Where(x => x.Route.AllowedHttpMethods.Any()).Each(Add);

            Get("icon").Action<FubuDiagnosticsEndpoint>(x => x.get__fubu_icon(null));
        }
    }
}