using System;
using System.IO;
using Baseline;
using Oakton;
using StoryTeller;
using ST.Client;
using StoryTeller.Model.Persistence;
using StoryTeller.Model.Persistence.DSL;
using StoryTeller.Remotes;

namespace ST.CommandLine
{
    public class ProjectInput
    {
        private readonly EngineMode _mode;

        public ProjectInput(EngineMode mode)
        {
            _mode = mode;

#if NET46
            Path = Environment.CurrentDirectory;
#else
            Path = Directory.GetCurrentDirectory();
#endif
        }

        [Description("Path to the StoryTeller project file or the project directory")]
        public string Path { get; set; }

        [Description("Override the dotnet framework if multi-targeting")]
        public string FrameworkFlag { get; set; }

#if NET46
        [Description("Directs Storyteller to run the system under test in a separate AppDomain if you are not using the Dotnet CLI")]
        public bool AppDomainFlag { get; set; } = false;
#endif

        [Description("Specify a build target to force Storyteller to choose that profile. By default, ST will use 'Debug'")]
        public string BuildFlag { get; set; }

        [Description("Storyteller test mode profile for systems like Serenity that use this")]
        public string ProfileFlag { get; set; }

        [Description("Optional. Default project timeout in seconds.")]
        public int? TimeoutFlag { get; set; }




        [Description("Optional. Override the config file selection of the Storyteller test running AppDomain")]
        public string ConfigFlag { get; set; }

        [Description("Optional. Override the spec directory")]
        [FlagAlias("specs", 's')]
        public string SpecsFlag { get; set; }



        [Description("Optional. Override the fixtures directory")]
        [FlagAlias("fixtures", 'f')]
        public string FixturesFlag { get; set; }


        public string SpecPath
        {
            get
            {
                if (SpecsFlag.IsNotEmpty())
                {
                    return SpecsFlag.ToFullPath();
                }

                return HierarchyLoader.SelectSpecPath(Path.ToFullPath());
            }
        }

        public string FixturePath
        {
            get
            {
                if (FixturesFlag.IsNotEmpty())
                {
                    return FixturesFlag.ToFullPath();
                }

                return FixtureLoader.SelectFixturePath(Path.ToFullPath());
            }
        }

        [Description("Force Storyteller to use this culture in all value conversions")]
        public string CultureFlag { get; set; }

        [Description("Optional. Tell Storyteller which which ISystem to use")]
        public string SystemNameFlag { get; set; }

        protected bool _disableAppDomainFileWatching = false;

        public EngineController BuildEngine()
        {
            var project = configureProject();

#if NET46
            var launcher = project.UseSeparateAppDomain 
                ? new AppDomainSystemLauncher(project)
                : (ISystemLauncher) new ProcessRunnerSystemLauncher(project);

            var controller = new EngineController(project, launcher);

            if (AppDomainFlag)
            {
                controller.DisableAppDomainFileWatching = _disableAppDomainFileWatching;
            }
            else
            {
                controller.DisableAppDomainFileWatching = true;
            }
#else
            var controller = new EngineController(project, new ProcessRunnerSystemLauncher(project));
            controller.DisableAppDomainFileWatching = true;
#endif

            return controller;
        }

        protected virtual Project configureProject()
        {
            var path = Path.ToFullPath();
            var project = Project.LoadForFolder(path);

            project.Framework = FrameworkFlag;

#if NET46
            project.UseSeparateAppDomain = AppDomainFlag;
#endif

            if (BuildFlag.IsNotEmpty())
            {
                project.BuildProfile = BuildFlag;
            }

            if (ConfigFlag.IsNotEmpty())
            {
                project.ConfigFile = ConfigFlag;
            }

            project.Culture = CultureFlag;


            if (TimeoutFlag.HasValue)
            {
                project.StopConditions.TimeoutInSeconds = TimeoutFlag.Value;
            }

            if (SystemNameFlag.IsNotEmpty())
            {
                project.SystemTypeName = SystemNameFlag;
            }

            project.Profile = ProfileFlag;

            project.Mode = _mode;

            return project;
        }

        public void CreateMissingSpecFolder()
        {
            if (!Directory.Exists(SpecPath))
            {
                Console.WriteLine("Creating specifications directory at " + SpecPath);
                Directory.CreateDirectory(SpecPath);
            }
        }

        

    }
}
