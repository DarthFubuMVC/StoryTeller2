using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Baseline.Reflection;
using StoryTeller.Conversion;
using StoryTeller.Model;
using StoryTeller.NewEngine;
using StoryTeller.Results;

namespace StoryTeller.Grammars.Reflection
{
    public class FactCheckMethod : IGrammar
    {
        private readonly MethodInvocation _invocation;

        public static FactCheckMethod For<T>(T target, Expression<Func<T, bool>> expression)
        {
            var method = ReflectionHelper.GetMethod(expression);
            var grammar = new FactCheckMethod(method, target);

            grammar.Compile(new Fixture(), CellHandling.Basic());

            return grammar;
        }

        public FactCheckMethod(MethodInfo method, object target)
        {
            _invocation = MethodInvocation.For(method, target);
        }

        public IExecutionStep CreatePlan(Step step, FixtureLibrary library, bool inTable = false)
        {
            var stepValues = _invocation.InputCells().ToStepValues(step);
            return new FactCheckPlan(stepValues, this);
        }

        public void CreatePlan(ExecutionPlan plan, Step step, FixtureLibrary library, bool inTable = false)
        {
            throw new NotImplementedException();
        }

        public GrammarModel Compile(Fixture fixture, CellHandling cells)
        {
            _invocation.Compile(fixture, cells);
            return new Sentence
            {
                format = _invocation.Format,
                cells = _invocation.InputCells().ToArray()
            };
        }

        public string Key { get; set; }
        public bool IsHidden { get; set; }


        public MethodInvocation Invocation => _invocation;
        public long MaximumRuntimeInMilliseconds { get; set; }


        public class FactCheckPlan : LineStepBase
        {
            private readonly FactCheckMethod _grammar;

            public FactCheckPlan(StepValues values, FactCheckMethod grammar)
                : base(values)
            {
                _grammar = grammar;
            }

            protected override StepResult execute(ISpecContext context)
            {
                var test = _grammar.Invocation.InvokeTest(Values);
                return new StepResult(Values.id, test ? ResultStatus.success : ResultStatus.failed);
            }

            public override string Subject => _grammar.Key;

            protected override async Task<StepResult> executeAsync(ISpecContext context)
            {
                var test = await _grammar.Invocation.InvokeTestAsync(Values).ConfigureAwait(false);
                return new StepResult(Values.id, test ? ResultStatus.success : ResultStatus.failed);
            }

            protected override bool IsAsync()
            {
                return _grammar.Invocation.IsAsync();
            }

            protected override long maximumRuntimeInMilliseconds => _grammar.MaximumRuntimeInMilliseconds;
        }
    }
}
