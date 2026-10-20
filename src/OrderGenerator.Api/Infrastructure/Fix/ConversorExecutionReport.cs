using OrderGenerator.Api.Application.Models;
using QuickFix.FIX44;

namespace OrderGenerator.Api.Infrastructure.Fix
{
    public sealed class ConversorExecutionReport
    {
        private readonly ILogger<ConversorExecutionReport> _logger;

        public ConversorExecutionReport(ILogger<ConversorExecutionReport> logger)
        {
            _logger = logger;
        }

        public ResultadoEnvioOrdem Converter(ExecutionReport relatorio)
        {
            string identificadorOrdem = relatorio.ClOrdID.Value;
            string simboloTexto = relatorio.Symbol.Value;

            if (!Enum.TryParse(simboloTexto, out SimboloOrdem simbolo))
            {
                throw new ArgumentException($"Símbolo inválido no ExecutionReport da ordem {identificadorOrdem}.");
            }

            char tipoExecucao = relatorio.ExecType.Value;
            char statusOrdem = relatorio.OrdStatus.Value;

            EstadoEnvioOrdem estado;

            string? motivo = relatorio.IsSetText() ? relatorio.Text.Value : null;

            if (tipoExecucao == QuickFix.Fields.ExecType.NEW && statusOrdem == QuickFix.Fields.OrdStatus.NEW)
                estado = EstadoEnvioOrdem.Aceita;
            else if (tipoExecucao == QuickFix.Fields.ExecType.REJECTED && statusOrdem == QuickFix.Fields.OrdStatus.REJECTED)
                estado = EstadoEnvioOrdem.Rejeitada;
            else
            {
                estado = EstadoEnvioOrdem.EstadoInesperado;

                motivo ??= $"Estado FIX inesperado: ExecType={tipoExecucao}, OrdStatus={statusOrdem}.";

                _logger.LogWarning("ExecutionReport em estado não suportado para {IdentificadorOrdem}: ExecType={ExecType}, OrdStatus={OrdStatus}.", identificadorOrdem, tipoExecucao, statusOrdem);
            }

            return new ResultadoEnvioOrdem
            {
                IdentificadorOrdem = identificadorOrdem,
                IdentificadorExecucao = relatorio.ExecID.Value,
                IdentificadorOrdemServidor = relatorio.OrderID.Value,
                Simbolo = simbolo,
                Estado = estado,
                MotivoRejeicao = motivo
            };
        }
    }
}
