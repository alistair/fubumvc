﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using FubuCore;
using FubuMVC.Core;
using FubuMVC.Core.Assets;
using FubuMVC.Core.Assets.JavascriptRouting;
using FubuMVC.Core.Diagnostics;
using FubuMVC.Core.Http;
using FubuMVC.Core.Registration;
using FubuMVC.Core.Runtime;
using FubuMVC.Core.Runtime.Files;
using HtmlTags;

namespace FubuMVC.Diagnostics
{
    [Tag("Diagnostics")]
    public class FubuDiagnosticsEndpoint
    {
        private readonly JavascriptRouteWriter _routeWriter;
        private readonly DiagnosticJavascriptRoutes _routes;
        private readonly IAssetGraph _assets;
        private readonly IHttpRequest _request;
        private readonly IFubuApplicationFiles _files;

        public FubuDiagnosticsEndpoint(JavascriptRouteWriter routeWriter, DiagnosticJavascriptRoutes routes, IAssetGraph assets, IHttpRequest request, IFubuApplicationFiles files)
        {
            _routeWriter = routeWriter;
            _routes = routes;
            _assets = assets;
            _request = request;
            _files = files;
        }

        private IEnumerable<Asset> findAssets(MimeType mimeType)
        {
            return _assets.Assets.Where(x => x.MimeType == mimeType && x.Url.StartsWith("fubu-diagnostics/"));
        } 

        public DashboardModel get__fubu()
        {
            var files = _files.FindFiles(FileSet.Deep("fubu-diagnostics/*.html"));
            var htmlTags = files.Select(x => {
                var contents = x.ReadContents();
                var tag = new HtmlTag("div").Id(Path.GetFileNameWithoutExtension(x.Path));
                tag.Encoded(false);
                tag.Text(contents);
                tag.Hide();
                tag.AddClass("left-content");

                return tag;
            }).ToArray();

            return new DashboardModel
            {
                StyleTags = findAssets(MimeType.Css).Select(x => new StylesheetLinkTag(_request.ToFullUrl(x.Url))).ToArray().ToTagList(),
                ScriptTags = findAssets(MimeType.Javascript).Select(x => new ScriptTag(_ => _request.ToFullUrl(_), x)).ToArray().ToTagList(),
                Router = _routeWriter.WriteJavascriptRoutes("FubuDiagnostics.routes", _routes),
                ReactTags = findAssets(MimeType.MimeTypeByValue("text/jsx")).Select(x => new ScriptTag(_ => _request.ToFullUrl(_), x).Attr("type", "text/jsx")).ToArray().ToTagList(),
                HtmlTags = htmlTags.ToTagList()
            };
        }
    }

    public class DiagnosticJavascriptRoutes : JavascriptRouter
    {
        public DiagnosticJavascriptRoutes(BehaviorGraph graph)
        {
            graph.Behaviors.OfType<DiagnosticChain>().Each(Add);
        }
    }

    public class DashboardModel
    {
        public TagList StyleTags { get; set; }
        public TagList ScriptTags { get; set; }
        public TagList ReactTags { get; set; }
        public HtmlTag Router { get; set; }

        public TagList HtmlTags { get; set; }
    }
}