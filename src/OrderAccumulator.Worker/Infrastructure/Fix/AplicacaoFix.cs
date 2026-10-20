using OrderAccumulator.Worker.Application.Models;
using OrderAccumulator.Worker.Application.Services;
using OrderAccumulator.Worker.Domain.Entities;
using QuickFix;
using QuickFix.FIX44;

namespace OrderAccumulator.Worker.Infrastructure.Fix
{
    public class AplicacaoFix : MessageCracker, IApplication
    {
        private readonly ProcessadorOrdens _processador;
        private readonly ConversorOrdemFix _conversor;
        private readonly FabricaExecutionReport _fabrica;
        private readonly ILogger<AplicacaoFix> _logger;

        public AplicacaoFix(ProcessadorOrdens processador,
                            ConversorOrdemFix conversor,
                            FabricaExecutionReport fabrica,
                            ILogger<AplicacaoFix> logger)
        {
            _processador = processador;
            _conversor = conversor;
            _fabrica = fabrica;
            _logger = logger;
        }

        public void OnCreate(SessionID sessionID)
        {
            _logger.LogInformation("Sessão FIX {Sessao} criada.", sessionID);
        }

        public void OnLogon(SessionID sessionID)
        {
            _logger.LogInformation("Logon da sessão FIX {Sessao}.", sessionID);
        }

        public void OnLogout(SessionID sessionID)
        {
            _logger.LogInformation("Logout da sessão FIX {Sessao}.", sessionID);
        }

        public void ToAdmin(QuickFix.Message mensagem, SessionID sessionID) { }
        public void FromAdmin(QuickFix.Message mensagem, SessionID sessionID) { }
        public void ToApp(QuickFix.Message mensagem, SessionID sessionID) { }

        public void FromApp(QuickFix.Message mensagem, SessionID sessionID)
        {
            Crack(mensagem, sessionID);
        }

        public void OnMessage(NewOrderSingle mensagem,
                              SessionID sessionID)
        {
            _logger.LogInformation("Ordem FIX recebida na sessão {Sessao}.", sessionID);

            bool aceita;
            string? motivo;
            Ordem? ordem = null;

            try
            {
                ordem = _conversor.Converter(mensagem);
            }
            catch (ArgumentException excecao)
            {
                _logger.LogWarning("Ordem FIX inválida: {Motivo}.", excecao.Message);
                EnviarRelatorio(mensagem,
                                sessionID,
                                false,
                                excecao.Message);
                return;
            }

            ResultadoProcessamentoOrdem resultado = _processador.ProcessarOrdem(ordem);

            aceita = resultado.Aceita;
            motivo = resultado.MotivoRejeicao;

            EnviarRelatorio(mensagem,
                            sessionID,
                            aceita,
                            motivo);
        }

        private void EnviarRelatorio(NewOrderSingle mensagem,
                                     SessionID sessionID,
                                     bool aceita,
                                     string? motivo)
        {
            ExecutionReport relatorio = _fabrica.Criar(mensagem,
                                                       aceita,
                                                       motivo);

            try
            {
                if (!Session.SendToTarget(relatorio, sessionID))
                {
                    _logger.LogError("Falha ao enviar ExecutionReport para a sessão {Sessao}.", sessionID);
                }
            }
            catch (SessionNotFound excecao)
            {
                _logger.LogError(excecao, "Sessão {Sessao} não encontrada ao enviar ExecutionReport.", sessionID);
            }
        }
    }
}
