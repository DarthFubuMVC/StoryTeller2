﻿using Baseline;
using StoryTeller.Conversion;
using StoryTeller.Grammars;
using StoryTeller.NewEngine;

namespace StoryTeller.Model
{
    public class ErrorGrammar : GrammarModel, IGrammar
    {
        private readonly string _key;
        private readonly string _message;

        public ErrorGrammar(string key, string error) : base("error")
        {
            _key = key;
            _message = "Grammar '{0}' is in an invalid state. See the grammar errors".ToFormat(_key);
            AddError(new GrammarError{error = error});
        }

        public IExecutionStep CreatePlan(Step step, FixtureLibrary library, bool inTable = false)
        {
            return new InvalidGrammarStep(new StepValues(step.id), _message);
        }

        public void CreatePlan(ExecutionPlan plan, Step step, FixtureLibrary library, bool inTable = false)
        {
            throw new System.NotImplementedException();
        }

        GrammarModel IGrammar.Compile(Fixture fixture, CellHandling cells)
        {
            return this;
        }

        public string Key { get; set; }
        public override string TitleOrFormat()
        {
            return "ERROR";
        }

        public long MaximumRuntimeInMilliseconds { get; set; }
    }
}
