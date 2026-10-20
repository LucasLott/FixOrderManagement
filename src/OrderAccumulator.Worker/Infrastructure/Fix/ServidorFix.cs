using QuickFix;
using QuickFix.Logger;
using QuickFix.Store;
using QuickFix.Transport;

namespace OrderAccumulator.Worker.Infrastructure.Fix
{
    public class ServidorFix : IHostedService
    {
        private readonly AplicacaoFix _aplicacao;
        private readonly ILogger<ServidorFix> _logger;
        private ThreadedSocketAcceptor? _acceptor;

        public ServidorFix(AplicacaoFix aplicacao, ILogger<ServidorFix> logger)
        {
            _aplicacao = aplicacao;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string pastaFix = Path.Combine(AppContext.BaseDirectory, "fix");
            var configuracao = new SessionSettings(Path.Combine(pastaFix, "acceptor.cfg"));

            foreach (SessionID sessao in configuracao.GetSessions())
            {
                SettingsDictionary parametros = configuracao.Get(sessao);
                parametros.SetString("DataDictionary", Path.Combine(pastaFix, "FIX44.xml"));
                parametros.SetString("FileStorePath", Path.Combine(pastaFix, "store"));
                parametros.SetString("FileLogPath", Path.Combine(pastaFix, "log"));
            }

            IMessageStoreFactory armazenamento = new FileStoreFactory(configuracao);
            ILogFactory log = new FileLogFactory(configuracao);

            _acceptor = new ThreadedSocketAcceptor(_aplicacao, armazenamento, configuracao, log);
            _acceptor.Start();
            
            _logger.LogInformation("Acceptor FIX iniciado na porta {Porta}.", 5001);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _acceptor?.Stop();
            _logger.LogInformation("Servidor FIX encerrado.");
            return Task.CompletedTask;
        }
    }
}
