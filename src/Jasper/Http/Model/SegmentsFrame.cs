﻿using System.Collections.Generic;
using Jasper.Codegen;
using Jasper.Codegen.Compilation;
using Jasper.Http.Routing.Codegen;

namespace Jasper.Http.Model
{
    public class SegmentsFrame : Frame
    {
        public SegmentsFrame() : base((bool) false)
        {
            Segments = new Variable(typeof(string[]), RoutingFrames.Segments, this);
            Creates = new[] {Segments};
        }

        public Variable Segments { get; }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            writer.WriteLine($"var {RoutingFrames.Segments} = (string[]){RouteGraph.Context}.Items[\"{RoutingFrames.Segments}\"];");

            Next?.GenerateCode(method, writer);
        }

        public override IEnumerable<Variable> Creates { get; }
    }
}
