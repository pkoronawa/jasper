﻿using BlueMilk.Codegen;
using BlueMilk.Compilation;
using Jasper.Http.Model;

namespace Jasper.Http.Routing.Codegen
{
    public class PathSegmentsFrame : RouteArgumentFrame
    {
        public PathSegmentsFrame(int position) : base(Route.PathSegments, position, typeof(string[]))
        {
        }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            writer.Write($"var {Variable.Usage} = {nameof(RouteHandler.ToPathSegments)}({Segments.Usage}, {Position});");
            Next?.GenerateCode(method, writer);
        }
    }
}
