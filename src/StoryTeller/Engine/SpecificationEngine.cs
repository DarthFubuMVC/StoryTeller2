﻿using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Baseline;
using Baseline.Dates;
using StoryTeller.Engine.Stepthrough;
using StoryTeller.Model;
using StoryTeller.Remotes;
using StoryTeller.Remotes.Messaging;

namespace StoryTeller.Engine
{
    public class SpecificationEngine : IDisposable, ISpecificationEngine
    {
        private readonly ConsumingQueue _executionQueue;
        private ConsumingQueue _planning;
        private readonly ISpecRunner _runner;
        private readonly RunningSystem _running;
        private readonly Task _warmup;
        private ConsumingQueue _reader;

        public SpecificationEngine(ISystem system, ISpecRunner runner, IExecutionObserver observer)
        {
            if (system == null) throw new ArgumentNullException(nameof(system));
            if (runner == null) throw new ArgumentNullException(nameof(runner));
            if (observer == null) throw new ArgumentNullException(nameof(observer));


            _running = RunningSystem.Create(system);
            _runner = runner;

            _executionQueue = new ConsumingQueue(request =>
            {
                if (request.IsCancelled)
                    return;

                _warmup.Wait(30.Seconds());

                var results = _runner.Execute(request, _executionQueue);

                if (!request.IsCancelled && (results != null))
                {
                    // TODO -- combine the two things here?
                    request.SpecExecutionFinished(results);
                    observer.SpecFinished(request);
                }
            });

            var warmup = _running.System.Warmup();

            if (warmup == null)
            {
                throw new InvalidOperationException($"{system} cannot return a null value from {nameof(ISystem.Warmup)}()");
            }


            if (warmup.Status == TaskStatus.WaitingForActivation)
            {
                warmup.Start();
            }

            _warmup = warmup.ContinueWith(t =>
            {
                if (t.IsFaulted)
                    _runner.MarkAsInvalid(t.Exception);
            });
        }

        public void Dispose()
        {
            _running.System.Dispose();
            _executionQueue.Dispose();
            _planning?.Dispose();
            _runner.Cancel();
        }

        public void Enqueue(SpecExecutionRequest request)
        {
            _planning.Enqueue(request);
        }

        public void CancelRunningSpec(string id)
        {
            _runner.Cancel(id);
        }

        public IStepthroughExecution CurrentStepthrough()
        {
            return _runner.Current as IStepthroughExecution;
        }


        private void startTheConsumingQueues(FixtureLibrary library)
        {
            _planning = new ConsumingQueue(request =>
            {
                var culture = Project.CurrentProject?.Culture;
                if (culture.IsNotEmpty())
                {
#if NET46
                    Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
#else
                    CultureInfo.CurrentCulture = new CultureInfo(culture);
#endif
                }

                request.CreatePlan(library);
                _executionQueue.Enqueue(request);
            });

            _planning.Start();
        }

        public void Start(StopConditions stopConditions)
        {
            _runner.UseStopConditions(stopConditions);
            _executionQueue.Start();

            if (_running.RecycledMessage.success)
            {
                startTheConsumingQueues(_running.Fixtures);
            }

            EventAggregator.SendMessage(_running.RecycledMessage);
        }


    }
}