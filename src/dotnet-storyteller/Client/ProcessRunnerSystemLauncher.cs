﻿using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Baseline;
using Baseline.Dates;
using Oakton;
using StoryTeller;
using StoryTeller.Engine;
using StoryTeller.Model;
using StoryTeller.Remotes;
using StoryTeller.Remotes.Messaging;

namespace ST.Client
{
    public class ProcessRunnerSystemLauncher : ISystemLauncher, IListener<AgentReady>
    {
        private readonly Project _project;
        private Process _process;
        private IEngineController _controller;
        private string _command;
        private bool _agentReady;
        private readonly object _readyLock = new object();
        private string _testCommand;

        public ProcessRunnerSystemLauncher(Project project)
        {
            _project = project;

            EventAggregator.Messaging.AddListener(this);
        }

        public void AssertValid()
        {
            // Nothing
        }

        private void killLingeringProcesses()
        {
            var processName = Path.GetFileName(_project.ProjectPath) + ".exe";
            var lingering = Process.GetProcessesByName(processName);
            foreach (var process in lingering)
            {
                ConsoleWriter.Write(ConsoleColor.Yellow, $"Killing process '{process.ProcessName}' #{process.Id}");
                process.Kill();
            }
        }

        public void Teardown()
        {
            if (_process == null) return;

            _controller.SendMessage(new Shutdown());

            _process.WaitForExit(5000);

            if (!_process.HasExited)
            {
                _process?.Kill();
            }
            
            ConsoleWriter.Write($"Shut down the spec running process at {_project.ProjectPath} with exit code {_process.ExitCode}");

            _process = null;

            killLingeringProcesses();
        }

        public void Start(IEngineController remoteController)
        {
            killLingeringProcesses();

            _controller = remoteController;

            // Watch UseShellExecute.
            var start = new ProcessStartInfo
            {
                UseShellExecute = false,
                WorkingDirectory = _project.ProjectPath.ToFullPath(),
                FileName = "dotnet"
            };


            var framework = _project.Framework;

#if NET46
            framework = framework ?? "net46";
#else
            framework = framework ?? "netcoreapp1.0";
#endif


            // TODO -- need to lock this down somehow
            start.Arguments = $"run --framework {framework} -- {_project.Port}";
            _testCommand = $"dotnet run --framework {framework} -- test";

            _command = $"dotnet {start.Arguments}";

            _process = Process.Start(start);
            _process.Exited += _process_Exited;

            lock (_readyLock)
            {
                _agentReady = false;
            }

            

            Task.Delay(5.Seconds()).ContinueWith(t =>
            {
                lock (_readyLock)
                {
                    if (!_agentReady)
                    {

                    }
                }

                if (_process.HasExited)
                {
                    sendFailedToStartMessage();
                }
            });

            if (_process.HasExited)
            {
                sendFailedToStartMessage();
            }
        }

        private void sendFailedToStartMessage()
        {
#if NET46
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
#else
            var baseDirectory = AppContext.BaseDirectory;
#endif

            var writer = new StringWriter();
            writer.WriteLine($"Unable to start process '{_command}'");
            writer.WriteLine();
            writer.WriteLine("Check the console output for details, or try this command in the root of the specification project:");
            writer.WriteLine();
            writer.WriteLine(_testCommand);
            writer.WriteLine();
            writer.WriteLine($"The error is logged to {baseDirectory.AppendPath("storyteller.log")}");

            var message = new SystemRecycled
            {
                success = false,
                fixtures = new FixtureModel[0],
                system_name = "Unknown",
                system_full_name = "Unknown",
                name = Path.GetFileName(baseDirectory),
                error = writer.ToString()
            };

            EventAggregator.SendMessage(message);
        }

        private void _process_Exited(object sender, EventArgs e)
        {
            if (_process.ExitCode != 0)
            {
                sendFailedToStartMessage();
            }
            
            

        }

        public void Receive(AgentReady message)
        {
            lock (_readyLock)
            {
                _agentReady = true;
            }

            ConsoleWriter.Write($"Agent ready at {_project.Port}.");
            _controller.SendMessage(new StartProject { Project = _project });

        }
    }
}