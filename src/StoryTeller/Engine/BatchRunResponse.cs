using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using StoryTeller.Messages;
using StoryTeller.Model;

namespace StoryTeller.Engine
{
    public class BatchRunResponse : ClientMessage
    {
        public BatchRunResponse() : base("batch-run-response")
        {
        }

        public BatchRecord[] records;
        public FixtureModel[] fixtures;

        public string time = DateTime.Now.ToString("g");
        public string system;
        public string suite;

        [JsonProperty("success")]
        public bool Success
        {
            get
            {
                if (records == null) return false;

                return !records.Any(x => x.specification.Lifecycle == Lifecycle.Regression && !x.WasSuccessful());
            }
        }

        public override string ToString()
        {
            var writer = new StringWriter();

            var regression = Summarize(Lifecycle.Regression);
            var acceptance = Summarize(Lifecycle.Acceptance);

            writer.WriteLine(acceptance);
            writer.WriteLine(regression);

            return writer.ToString();
        }

        public LifecycleSummary Summarize(Lifecycle lifecycle)
        {
            return new LifecycleSummary
            {
                Lifecycle = lifecycle,
                Successful = records.Count(x => x.specification.Lifecycle == lifecycle && x.WasSuccessful()),
                Failed = records.Count(x => x.specification.Lifecycle == lifecycle && !x.WasSuccessful())
            };
        }
    }
}