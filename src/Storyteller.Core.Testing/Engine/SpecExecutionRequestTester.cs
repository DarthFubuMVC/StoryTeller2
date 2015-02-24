﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FubuCore;
using FubuTestingSupport;
using NUnit.Framework;
using Rhino.Mocks;
using Storyteller.Core.Engine;
using Storyteller.Core.Messages;
using Storyteller.Core.Model;
using Storyteller.Core.Model.Persistence;
using Storyteller.Core.Remotes.Messaging;

namespace Storyteller.Core.Testing.Engine
{
    [TestFixture]
    public class SpecExecutionRequestTester
    {
        private SpecNode theSpec;
        private RuntimeErrorListener listener;

        public SpecExecutionRequestTester()
        {
            var path = ".".ToFullPath().ParentDirectory().ParentDirectory().ParentDirectory()
                .AppendPath("StoryTeller.Samples", "Specs", "General", "Check properties.xml");

            theSpec = HierarchyLoader.ReadSpecNode(path);
        }

        [SetUp]
        public void SetUp()
        {
            listener = new RuntimeErrorListener();
        }

        [TearDown]
        public void TearDown()
        {
            EventAggregator.Messaging.RemoveListener(listener);
        }

        [Test]
        public void finishing_a_spec()
        {
            var action = MockRepository.GenerateMock<IResultObserver>();

            var request = new SpecExecutionRequest(theSpec, action);
            var context = SpecContext.Basic();

            request.SpecExecutionFinished(context);

            action.AssertWasCalled(x => x.SpecExecutionFinished(theSpec, context.Counts));
        }

        [Test]
        public void read_xml_happy_path()
        {
            var request = SpecExecutionRequest.For(theSpec);

            request.ReadXml();

            request.Specification.ShouldNotBeNull();
            request.Specification.Children.Count.ShouldBeGreaterThan(0);

            request.IsCancelled.ShouldBeFalse();
        }

        [Test]
        public void read_xml_sad_path()
        {
            var request = SpecExecutionRequest.For(new SpecNode {Filename = "nonexistent.xml"});

            EventAggregator.Messaging.AddListener(listener);

            request.ReadXml();

            request.IsCancelled.ShouldBeTrue();

            var error = listener.Errors.Single();

            error.error.ShouldContain("System.IO.FileNotFoundException");

        }

        [Test]
        public void cancel_cancels_the_request()
        {
            var request = SpecExecutionRequest.For(theSpec);
            request.IsCancelled.ShouldBeFalse();

            request.Cancel();

            request.IsCancelled.ShouldBeTrue();
        }

        [Test]
        public void cancel_cancels_the_cancellation_token_if_the_context_exista()
        {
            var request = SpecExecutionRequest.For(theSpec);
            var context = request.CreateContext(new StopConditions(),
                new NulloSystem.SimpleExecutionContext(new InMemoryServiceLocator()));

            request.Cancel();

            context.Cancellation.IsCancellationRequested.ShouldBeTrue();
        }

        [Test]
        public void create_plan_happy_path_smoke_test()
        {
            var request = SpecExecutionRequest.For(theSpec);
            request.ReadXml();
            request.CreatePlan(TestingContext.Library);

            request.IsCancelled.ShouldBeFalse();

            request.Plan.ShouldNotBeNull();
        }

        [Test]
        public void create_plan_sad_path_smoke_test()
        {
            var request = SpecExecutionRequest.For(theSpec);
            
            // No specification, so it blows up
            //request.ReadXml();
            request.CreatePlan(TestingContext.Library);

            request.IsCancelled.ShouldBeTrue();
        }

        public class RuntimeErrorListener : IListener<RuntimeError>
        {
            public readonly IList<RuntimeError> Errors = new List<RuntimeError>();

            public void Receive(RuntimeError message)
            {
                Errors.Add(message);
            }
        }
    }
}