﻿using System;
using System.Reflection;
using System.Threading.Tasks;
using Baseline;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using StorytellerRunner;
using StoryTeller;
using StoryTeller.Messages;
using StoryTeller.Model;
using StoryTeller.Remotes;

namespace ST.Client
{
    public interface IApplication : IDisposable
    {
        IEngineController Engine { get; }
        IClientConnector Client { get; }
        IPersistenceController Persistence { get; }
        SystemRecycled LatestSystemRecycled { get; }
        IFixtureController Fixtures { get; }
        QueueState QueueState { get; }
        Batch BuildInitialModel();
    }

    public interface IWebApplicationRunner : IDisposable
    {
        IClientConnector Start(IApplication application);
        string BaseAddress { get; }
    }

    public class WebApplicationRunner : IWebApplicationRunner
    {
        private readonly OpenInput _input;
        private FixtureController _fixtures;
        private IWebHost _server;
#if DEBUG
        private AssetFileWatcher _watcher;
#endif
        private CommandRunner _commands;
        private IApplication _application;

        public WebApplicationRunner(OpenInput input)
        {
            _input = input;
        }


        public string BaseAddress { get; private set; }


        public IClientConnector Client { get; private set; }


        public void Dispose()
        {
            _server.SafeDispose();
#if DEBUG
            _watcher?.Dispose();
#endif
        }



        public IClientConnector Start(IApplication application)
        {
            var port = PortFinder.FindPort(5000);

            _application = application;

            BaseAddress = "http://localhost:" + port;

            var webSockets = new WebSocketsHandler();

            _commands = new CommandRunner(application);


            Client = new ClientConnector(webSockets, _commands.HandleJson)
            {
                WebSocketsAddress = $"ws://127.0.0.1:{port}"
            };

            startWebServer(port, webSockets);

#if DEBUG
            _watcher = new AssetFileWatcher(Client);
            _watcher.Start(_input);
#endif

            return Client;
        }

        private void startWebServer(int port, WebSocketsHandler webSockets)
        {
#if NET46
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
#else
            var baseDirectory = AppContext.BaseDirectory;
#endif
            var hostBuilder = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(baseDirectory)
                .Configure(app =>
                {
                    app.UseWebSockets();

                    app.Use(async (http, next) =>
                    {
                        if (http.WebSockets.IsWebSocketRequest)
                            await webSockets.HandleSocket(http).ConfigureAwait(false);
                        else
                            await next().ConfigureAwait(false);
                    });

#if DEBUG
                    configureStaticFiles(app);
#endif

                    app.Run(async http =>
                    {
                        if (http.Request.Path == "/favicon.ico")
                        {
                            await writeFavicon(http).ConfigureAwait(false);

                            return;
                        }

                        try
                        {
                            http.Response.ContentType = "text/html";

                            await HomeEndpoint.BuildPage(http.Response, _application, _input).ConfigureAwait(false);


                            
                            
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.ToString());
                            throw;
                        }



                    });
                });

            _server = hostBuilder.Start($"http://localhost:{port}");
        }

        private async Task writeFavicon(HttpContext http)
        {
            var stream =
                GetType()
                    .GetTypeInfo()
                    .Assembly.GetManifestResourceStream("StorytellerRunner.favicon.ico");

            http.Response.ContentType = "image/x-icon";
            await stream.CopyToAsync(http.Response.Body).ConfigureAwait(false);
        }

        private void configureStaticFiles(IApplicationBuilder app)
        {
            var baseDirectory = AssetFileWatcher.FindRootFolder();

            var clientRoot = AssetFileWatcher.FindClientFolder();

            app.UseStaticFiles(new StaticFileOptions
            {
                ServeUnknownFileTypes = true,
                FileProvider =
                    new CompositeFileProvider(new PhysicalFileProvider(baseDirectory),
                        new PhysicalFileProvider(clientRoot))
            });
        }
    }
}