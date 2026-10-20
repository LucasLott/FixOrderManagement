using Microsoft.Extensions.Logging;
using OrderAccumulator.Worker.Application.Models;
using OrderAccumulator.Worker.Domain.Entities;
using OrderAccumulator.Worker.Infrastructure.Memory;

namespace OrderAccumulator.Worker.Application.Services
{
    public class ProcessadorOrdens
    {
        private const string MotivoLimiteExcedido = "A ordem ultrapassa o limite de exposição financeira para o símbolo.";
        private const string MotivoConflitoIdentificador = "O identificador da ordem já foi utilizado com dados financeiros diferentes.";

        private readonly CalculadoraExposicao _calculadoraExposicao;
        private readonly ArmazenamentoExposicao _armazenamentoExposicao;
        private readonly ILogger<ProcessadorOrdens> _logger;

        public ProcessadorOrdens(CalculadoraExposicao calculadoraExposicao,
                                 ArmazenamentoExposicao armazenamentoExposicao,
                                 ILogger<ProcessadorOrdens> logger)
        {
            _calculadoraExposicao = calculadoraExposicao;
            _armazenamentoExposicao = armazenamentoExposicao;
            _logger = logger;
        }

        public ResultadoProcessamentoOrdem ProcessarOrdem(Ordem ordem)
        {
            ArgumentNullException.ThrowIfNull(ordem);
            _logger.LogInformation("Ordem {IdentificadorOrdem} recebida para processamento.", ordem.Identificador);

            ResultadoProcessamentoOrdem resultado = _armazenamentoExposicao.ProcessarAtomicamente(ordem,
                                                                                                  exposicaoAtual => CriarResultadoProcessamento(ordem, exposicaoAtual),
                                                                                                  () => CriarResultadoConflito(ordem),
                                                                                                  possuiConflito => RegistrarDuplicidade(ordem, possuiConflito));

            RegistrarResultado(ordem,
                               resultado);

            return resultado;
        }

        private ResultadoProcessamentoOrdem CriarResultadoProcessamento(Ordem ordem,
                                                                        decimal exposicaoAtual)
        {
            decimal exposicaoProjetada = _calculadoraExposicao.CalcularExposicaoProjetada(exposicaoAtual,
                                                                                          ordem);

            if (!_calculadoraExposicao.EstaDentroDoLimite(exposicaoProjetada))
            {
                return new ResultadoProcessamentoOrdem(ordem.Identificador,
                                                       ordem.Simbolo,
                                                       false,
                                                       exposicaoAtual,
                                                       exposicaoProjetada,
                                                       MotivoLimiteExcedido);
            }

            return new ResultadoProcessamentoOrdem(ordem.Identificador,
                                                   ordem.Simbolo,
                                                   true,
                                                   exposicaoProjetada,
                                                   exposicaoProjetada);
        }

        private static ResultadoProcessamentoOrdem CriarResultadoConflito(Ordem ordem)
        {
            return new ResultadoProcessamentoOrdem(ordem.Identificador,
                                                   ordem.Simbolo,
                                                   false,
                                                   0m,
                                                   0m,
                                                   MotivoConflitoIdentificador);
        }

        private void RegistrarResultado(Ordem ordem,
                                        ResultadoProcessamentoOrdem resultado)
        {
            if (resultado.Aceita)
            {
                _logger.LogInformation("Ordem {IdentificadorOrdem} aceita.", ordem.Identificador);
                return;
            }

            if (resultado.MotivoRejeicao == MotivoConflitoIdentificador)
            {
                return;
            }

            _logger.LogWarning("Ordem {IdentificadorOrdem} rejeitada por limite.", ordem.Identificador);
        }

        private void RegistrarDuplicidade(Ordem ordem,
                                          bool possuiConflito)
        {
            if (possuiConflito)
            {
                _logger.LogWarning("Conflito de identificador para a ordem {IdentificadorOrdem}.", ordem.Identificador);
                return;
            }

            _logger.LogInformation("Ordem duplicada {IdentificadorOrdem} recebida.", ordem.Identificador);
        }
    }
}
