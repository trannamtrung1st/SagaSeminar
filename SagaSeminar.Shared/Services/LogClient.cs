using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;
using SagaSeminar.Shared.Models;
using SagaSeminar.Shared.Service.Interfaces;
using SagaSeminar.Shared.Utils;

namespace SagaSeminar.Shared.Service
{
    public class LogClient : ILogClient, IDisposable
    {
        private readonly SemaphoreSlim _semaphoreSlim;
        private readonly IOptions<ApiInfo> _apiInfoOptions;
        private readonly string _defaultColor;
        private readonly string _source;
        private HubConnection _connection;
        private bool _disposedValue;

        public LogClient(IOptions<ApiInfo> apiInfoOptions,
            string defaultColor,
            string source)
        {
            _semaphoreSlim = new SemaphoreSlim(1);
            _apiInfoOptions = apiInfoOptions;
            _defaultColor = defaultColor;
            _source = source;
        }

        public async Task Log(string log)
        {
            LogModel model = new LogModel
            {
                Data = log,
                Color = _defaultColor,
                Source = _source,
                Time = DateTime.Now
            };

            await EnsureInitialization();

            await _connection.InvokeAsync(nameof(Log), model);
        }

        public async Task<IDisposable> HandleLog(Func<LogModel, Task> handleLog)
        {
            await EnsureInitialization();

            return _connection.On(nameof(HandleLog), handleLog);
        }

        private async Task EnsureInitialization()
        {
            try
            {
                _semaphoreSlim.Wait();

                if (_connection == null)
                {
                    _connection = new HubConnectionBuilder()
                        .WithUrl(new Uri(new Uri(_apiInfoOptions.Value.GlobalApiBase), "/hub/logs"), opt =>
                        {
                            if (!RuntimeHelper.IsWebAssembly())
                            {
                                opt.HttpMessageHandlerFactory = (handler) =>
                                {
                                    if (handler is HttpClientHandler clientHandler)
                                        // [DEMO] for using self-signed certificate inside Docker
                                        clientHandler.ServerCertificateCustomValidationCallback +=
                                            (sender, certificate, chain, sslPolicyErrors) => true;
                                    return handler;
                                };
                            }
                        })
                        .WithAutomaticReconnect()
                        .Build();

                    await _connection.StartAsync();
                }
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                }

                _connection?.StopAsync().Wait();
                _connection?.DisposeAsync().AsTask().Wait();

                _disposedValue = true;
            }
        }

        ~LogClient()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
