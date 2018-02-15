﻿using System;
using BlueMilk.Codegen;
using BlueMilk.Compilation;
using Jasper.Http.Model;

namespace Jasper.Http.Routing.Codegen
{
    public class RelativePathFrame : RouteArgumentFrame
    {
        public RelativePathFrame(int position) : base(Route.RelativePath, position, typeof(string))
        {

        }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            writer.Write($"var {Variable.Usage} = {nameof(RouteHandler.ToRelativePath)}({Segments.Usage}, {Position});");
            Next?.GenerateCode(method, writer);
        }
    }
}
