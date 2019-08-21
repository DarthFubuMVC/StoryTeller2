﻿using System;
using System.Threading.Tasks;
using StoryTeller.Conversion;
using StoryTeller.Model;
using StoryTeller.NewEngine;
using StoryTeller.Results;

namespace StoryTeller.Grammars
{
    public class FactGrammar : IFactGrammar
    {
        private readonly string _label;
        private readonly Func<ISpecContext, bool> _test;

        public FactGrammar(string label, Func<ISpecContext, bool> test)
        {
            _label = label;
            _test = test;
        }

        public string Key { get; set; }

        IExecutionStep IGrammar.CreatePlan(Step step, FixtureLibrary library, bool inTable)
        {
            return new FactPlan(new StepValues(step.id), this);
        }

        public void CreatePlan(ExecutionPlan plan, Step step, FixtureLibrary library, bool inTable = false)
        {
            var line = new LineExecution(step, (c, r) =>
            {
                
            });
        }

        public GrammarModel Compile(Fixture fixture, CellHandling cells)
        {
            return new Sentence
            {
                fact = true,
                format = _label,
                cells = new Cell[0]
            };
        }

        public bool PerformTest(StepValues values, ISpecContext context)
        {
            return _test(context);
        }

        public bool IsHidden { get; set; }
        public bool IsAsync()
        {
            return false;
        }

        [Obsolete]
        public Task<bool> PerformTestAsync(StepValues values, ISpecContext context)
        {
            throw new NotImplementedException();
        }

        public long MaximumRuntimeInMilliseconds { get; set; }
    }

    public interface IFactGrammar : IGrammar
    {
        bool PerformTest(StepValues values, ISpecContext context);

        bool IsAsync();

        Task<bool> PerformTestAsync(StepValues values, ISpecContext context);

    }

    [Obsolete]
    public class FactPlan : LineStepBase, IWithValues
    {
        private readonly IFactGrammar _grammar;

        public FactPlan(StepValues values, IFactGrammar grammar) : base(values)
        {
            _grammar = grammar;
        }

        public override string Subject => _grammar.Key;

        protected override StepResult execute(ISpecContext context)
        {
            var test = _grammar.PerformTest(Values, context);
            return new StepResult(Values.id, test ? ResultStatus.success : ResultStatus.failed);
        }

        protected override Task<StepResult> executeAsync(ISpecContext context)
        {
            return _grammar.PerformTestAsync(Values, context).ContinueWith(t =>
            {
                return t.IsFaulted 
                    ? new StepResult(Values.id, t.Exception) 
                    : new StepResult(Values.id, t.Result ? ResultStatus.success : ResultStatus.failed);
            });
        }

        protected override bool IsAsync()
        {
            return _grammar.IsAsync();
        }

        protected override long maximumRuntimeInMilliseconds => _grammar.MaximumRuntimeInMilliseconds;
    }
}
