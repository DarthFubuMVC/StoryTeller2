﻿using System;
using System.Collections.Generic;
using System.Linq;
using Baseline;
using StoryTeller.Grammars.ObjectBuilding;
using StoryTeller.Model;
using StoryTeller.NewEngine;

namespace StoryTeller.Grammars.Paragraphs
{
    public class ParagraphGrammar : IGrammar
    {
        private readonly string _title;
        private string _key;

        public ParagraphGrammar(string title)
        {
            _title = title;
        }

        public IExecutionStep CreatePlan(Step step, FixtureLibrary library, bool inTable = false)
        {
            var children = Children.Select(x => x.CreatePlan(step, library)).ToArray();

            if (inTable && children.All(x => x is ILineExecution))
            {
                return new AggregateLineExecution(children.OfType<ILineExecution>());
            }

            return new CompositeExecution(children);
        }

        public void CreatePlan(ExecutionPlan plan, Step step, FixtureLibrary library, bool inTable = false)
        {
            throw new NotImplementedException();
        }

        public IList<IGrammar> Children { get; } = new List<IGrammar>();

        /// <summary>
        /// Adds a new child grammar to this ParagraphGrammar
        /// </summary>
        /// <param name="grammar"></param>
        public void AddGrammar(IGrammar grammar)
        {
            Children.Add(grammar);
        }

        public GrammarModel Compile(Fixture fixture, CellHandling cells)
        {
            var childrenModels = Children.Select(_ =>
            {
                var child = _.Compile(fixture, cells);
                child.key = _.Key;

                return child;
            }).ToArray();

            return new Paragraph (childrenModels){ title = _title};
        }

        public void Do(Action<ISpecContext> action)
        {
            var silent = new SilentGrammar(Children.Count, action);
            Children.Add(silent);
        }


        public string Key
        {
            get { return _key; }
            set
            {
                _key = value;
                for (var i = 0; i < Children.Count; i++)
                {
                    var child = Children[i];
                    if (child.Key.IsEmpty())
                    {
                        child.Key = value + ":" + i;
                    }
                }
            }
        }

        public bool IsHidden { get; set; }
        public long MaximumRuntimeInMilliseconds { get; set; }
    }


    public class ParagraphBuilder
    {
        private readonly ParagraphGrammar _grammar;

        public ParagraphBuilder(ParagraphGrammar grammar)
        {
            _grammar = grammar;
        }

        public static ParagraphBuilder operator +(ParagraphBuilder expression, IGrammar grammar)
        {
            expression._grammar.AddGrammar(grammar);
            return expression;
        }

        public static ParagraphBuilder operator +(ParagraphBuilder expression, Action action)
        {
            expression._grammar.Do(c => action());

            return expression;
        }

        public static ParagraphBuilder operator +(ParagraphBuilder expression, Action<ISpecContext> action
            )
        {
            expression._grammar.Do(action);

            return expression;
        }

        public void VerifyPropertiesOf<T>(Action<ObjectVerificationExpression<T>> action)
            where T : class
        {
            var expression = new ObjectVerificationExpression<T>(_grammar);
            action(expression);
        }

        public void SetPropertiesOnCurrentObject<T>(Action<ObjectConstructionExpression<T>> action)
        {
            var expression = new ObjectConstructionExpression<T>(_grammar);
            action(expression);
        }
    }


}
