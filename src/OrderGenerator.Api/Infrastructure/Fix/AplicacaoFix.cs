using OrderGenerator.Api.Application.Services;
using QuickFix;
using QuickFix.FIX44;

namespace OrderGenerator.Api.Infrastructure.Fix
{
    public sealed class AplicacaoFix : MessageCracker, IApplication
    {
        private readonly ConversorExecutionReport _conversor;
        private readonly GerenciadorSolicitacoesOrdens _gerenciador;
        private readonly ILogger<AplicacaoFix> _logger;
        private readonly Action<bool> _alterarEstadoSessao;

        public AplicacaoFix(ConversorExecutionReport conversor,
                            GerenciadorSolicitacoesOrdens gerenciador,
                            Action<bool> alterarEstadoSessao,
                            ILogger<AplicacaoFix> logger)
        {
            _conversor = conversor;
            _gerenciador = gerenciador;
            _alterarEstadoSessao = alterarEstadoSessao;
            _logger = logger;
        }

        public void OnCreate(SessionID sessionID)
        {
            _logger.LogInformation("Sessão FIX criada: {Sessao}.", sessionID);
        }

        public void OnLogon(SessionID sessionID)
        {
            _alterarEstadoSessao(true);
            _logger.LogInformation("Logon FIX concluído: {Sessao}.", sessionID);
        }

        public void OnLogout(SessionID sessionID)
        {
            _alterarEstadoSessao(false);
            _logger.LogInformation("Logout FIX: {Sessao}.", sessionID);
        }

        public void ToAdmin(QuickFix.Message mensagem, SessionID sessionID)
        {
        }

        public void FromAdmin(QuickFix.Message mensagem, SessionID sessionID)
        {
        }

        public void ToApp(QuickFix.Message mensagem, SessionID sessionID)
        {
        }

        public void FromApp(QuickFix.Message mensagem, SessionID sessionID)
        {
            Crack(mensagem, sessionID);
        }

        public void OnMessage(ExecutionReport relatorio, SessionID sessionID)
        {
            try
            {
                var resultado = _conversor.Converter(relatorio);

                _logger.LogInformation("ExecutionReport recebido para {IdentificadorOrdem}, estado {Estado}.", resultado.IdentificadorOrdem, resultado.Estado);
                
                _gerenciador.Concluir(resultado);
            }
            catch (Exception excecao)
            {
                _logger.LogError(excecao, "Falha ao interpretar ExecutionReport na sessão {Sessao}.", sessionID);
            }
        }
    }
}
