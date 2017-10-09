﻿using System;
using System.Threading;
using Baseline;
using Oakton;
using StorytellerDocGen.Exporting;
using StorytellerDocGen.Topics;
using StoryTeller.Util;
using HtmlTextWriter = StoryTeller.Util.HtmlTextWriter;

namespace StorytellerDocGen.Transformation
{
    public interface IHtmlGenerator
    {
        string Generate(Topic topic);
    }

    public class HtmlGenerator : IHtmlGenerator
    {
        static HtmlGenerator()
        {
            TagRegister.Register("nav");
            TagRegister.Register("em");
            TagRegister.Register("section");
        }

        private readonly ITransformer _transformer;
        private readonly DocSettings _settings;

        public HtmlGenerator(DocSettings settings, ITransformer transformer)
        {
            _transformer = transformer;
            _settings = settings;
        }

        public string Generate(Topic topic)
        {
            if (topic.IsSplashPage())
            {
                return _transformer.Transform(topic, new FileSystem().ReadStringFromFile(topic.File));
            }

            try
            {
                return generate(topic);
            }
            catch (Exception e)
            {
                Exporter.Warnings.Add("Failed to transform topic at " + topic.File);
                
                ConsoleWriter.Write(ConsoleColor.Yellow, "Failed to transform topic at " + topic.File);
                ConsoleWriter.Write(ConsoleColor.Red, e.ToString());

                var document = new HtmlDocument
                {
                    Title = "Error!"
                };

                document.Add("h1").Text("Error!");

                document.Add("pre").Text(e.ToString());


                return document.ToString();
            }
        }

        private string generate(Topic topic)
        {
            try
            {
                var template = readTemplate();

                return _transformer.Transform(topic, template);
            }
            catch (Exception)
            {
                Thread.Sleep(100);

                // One retry because of over-eager file locking
                var template = readTemplate();

                return _transformer.Transform(topic, template);
            }
        }

        private string readTemplate()
        {
            return new FileSystem().ReadStringFromFile(_settings.Root.AppendPath("layout.htm"));
        }

        private class TagRegister : HtmlTextWriter
        {
            private TagRegister() : base(null) { }

            public static void Register(string tagName)
            {
                try
                {
                    RegisterTag(tagName, HtmlTextWriterTag.Unknown);
                }
                catch (Exception)
                {
                    // don't care
                }
            }
        }

    }
}