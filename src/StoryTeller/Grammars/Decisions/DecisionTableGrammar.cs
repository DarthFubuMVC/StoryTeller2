using StoryTeller.Grammars.Tables;
using StoryTeller.Model;
using StoryTeller.NewEngine;

namespace StoryTeller.Grammars.Decisions
{
    public abstract class DecisionTableGrammar : IGrammar, IBeforeAndAfter
    {
        private readonly TableGrammar _inner;

        public DecisionTableGrammar(string label)
        {
            _inner = new TableGrammar(new TableLineGrammar(this))
                .Titled(label).LeafName("table");
        }

        public string Key { get; set; }

        void IBeforeAndAfter.BeforeLine()
        {
            beforeLine();
        }

        void IBeforeAndAfter.AfterLine()
        {
            afterLine();
        }

        protected virtual void beforeLine()
        {
        }

        protected virtual void afterLine()
        {
        }

        public IExecutionStep CreatePlan(Step step, FixtureLibrary library, bool inTable = false)
        {
            return _inner.CreatePlan(step, library);
        }

        public void CreatePlan(ExecutionPlan plan, Step step, FixtureLibrary library, bool inTable = false)
        {
            throw new System.NotImplementedException();
        }

        public GrammarModel Compile(Fixture fixture, CellHandling cells)
        {
            return _inner.Compile(fixture, cells);
        }

        public bool IsHidden { get; set; }
        public long MaximumRuntimeInMilliseconds { get; set; }
    }
}
