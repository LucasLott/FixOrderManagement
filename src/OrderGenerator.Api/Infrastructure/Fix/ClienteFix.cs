using OrderGenerator.Api.Application.Models;
using Microsoft.Extensions.Hosting;
using QuickFix;
using QuickFix.Logger;
using QuickFix.Store;
using QuickFix.Transport;
using QuickFix.FIX44;

namespace OrderGenerator.Api.Infrastructure.Fix
{
    public sealed class ClienteFix : IHostedService
    {
        private static ClienteFix? _instanciaAtual;
        private AplicacaoFix? _aplicacao;
        private readonly ConversorOrdemFix _conversor;
        private readonly ILogger<ClienteFix> _logger;
        private readonly string _pastaFix;
        private readonly SessionID _sessionID;
        private readonly IServiceProvider _provedor;
        private bool _sessaoAtiva;
        private SocketInitiator? _initiator;

        public ClienteFix(ConversorOrdemFix conversor,
                          ILogger<ClienteFix> logger,
                          IHostEnvironment ambiente,
                          IServiceProvider provedor)
        {
            _conversor = conversor;
            _logger = logger;
            _pastaFix = Path.Combine(ambiente.ContentRootPath, "fix");
            _sessionID = new SessionID("FIX.4.4", "GENERATOR", "ACCUMULATOR");
            _provedor = provedor;
        }

        internal static ClienteFix? InstanciaAtual => Volatile.Read(ref _instanciaAtual);

        public Task StartAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string caminhoConfiguracao = Path.Combine(_pastaFix, "initiator.cfg");

            var configuracao = new SessionSettings(caminhoConfiguracao);

            _aplicacao = _provedor.GetRequiredService<AplicacaoFix>();

            foreach (SessionID sessao in configuracao.GetSessions())
            {
                SettingsDictionary parametros = configuracao.Get(sessao);

                parametros.SetString("DataDictionary", Path.Combine(_pastaFix, "FIX44.xml"));
                parametros.SetString("FileStorePath", Path.Combine(_pastaFix, "initiator-store"));
                parametros.SetString("FileLogPath", Path.Combine(_pastaFix, "initiator-log"));
            }

            IMessageStoreFactory armazenamento = new FileStoreFactory(configuracao);

            ILogFactory log = new FileLogFactory(configuracao);

            _initiator = new SocketInitiator(_aplicacao, armazenamento, configuracao, log);

            Interlocked.Exchange(ref _instanciaAtual, this);

            try
            {
                _logger.LogInformation("Iniciando FIX Initiator com configuração {CaminhoConfiguracao}.", caminhoConfiguracao);
                _initiator.Start();
            }
            catch (Exception excecao)
            {
                _logger.LogError(excecao, "Falha ao iniciar o FIX Initiator.");

                Interlocked.CompareExchange(ref _instanciaAtual, null, this);

                _initiator.Dispose();
                _initiator = null;
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Interlocked.CompareExchange(ref _instanciaAtual, null, this);

            _initiator?.Stop();
            _initiator?.Dispose();
            _initiator = null;
            _logger.LogInformation("FIX Initiator encerrado.");
            return Task.CompletedTask;
        }

        internal bool Enviar(SolicitacaoOrdem solicitacao)
        {
            SocketInitiator? initiator = _initiator;

            if (initiator is null || !_sessaoAtiva)
                return false;

            NewOrderSingle mensagem = _conversor.Converter(solicitacao);

            _logger.LogInformation("Enviando NewOrderSingle {IdentificadorOrdem} pela sessão {Sessao}.", solicitacao.Identificador, _sessionID);
            return Session.SendToTarget(mensagem, _sessionID);
        }

        internal void DefinirSessaoAtiva(bool ativa)
        {
            _sessaoAtiva = ativa;
        }
    }
}
