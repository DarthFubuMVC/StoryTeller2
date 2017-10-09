﻿using System;
using StorytellerDocGen.Exporting;
using StorytellerDocGen.Samples;
using StorytellerDocGen.Topics;
using StoryTeller.Util;

namespace StorytellerDocGen.Transformation
{
    public class CodeSampleTransformHandler : ITransformHandler
    {
        private readonly ISampleCache _cache;

        public CodeSampleTransformHandler(ISampleCache cache)
        {
            _cache = cache;
        }

        public string Key => "sample";

        public string Transform(Topic current, string data)
        {
            var tag = TagForSample(data);

            if (tag is MissingSampleTag)
            {
                Exporter.Warnings.Add($"Could not find sample '{data}' referenced in file {current.File}");
            }

            var subject = "<p>" + Guid.NewGuid().ToString() + "</p>";

            current.Substitutions[subject] = tag.ToString();

            return subject;
        }

        public HtmlTag TagForSample(string sampleName)
        {
            var sample = _cache.Find(sampleName.Trim());

            
            return sample == null ? (HtmlTag) new MissingSampleTag(sampleName) : new SampleTag(sample);
        }
    }

    public class MissingSampleTag : HtmlTag
    {
        public MissingSampleTag(string sampleName) : base("p")
        {
            Style("padding", "10px");
            AddClass("bg-warning");
            Add("b").Text($"Missing code sample '{sampleName}'");
            Add("small").Text(" -- Wait for dotnet stdocs to catch up reading samples or CTRL+SHIFT+R to force refresh");
        }
    }
}