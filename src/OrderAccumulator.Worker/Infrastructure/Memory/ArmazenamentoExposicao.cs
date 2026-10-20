using System.Collections.Concurrent;
using OrderAccumulator.Worker.Application.Models;
using OrderAccumulator.Worker.Domain.Entities;
using OrderAccumulator.Worker.Domain.Enums;

namespace OrderAccumulator.Worker.Infrastructure.Memory
{
    public class ArmazenamentoExposicao
    {
        private readonly object _bloqueio = new();
        private readonly ConcurrentDictionary<SimboloAtivo, decimal> _exposicoes = new();
        private readonly ConcurrentDictionary<string, Ordem> _ordensProcessadas = new();
        private readonly ConcurrentDictionary<string, ResultadoProcessamentoOrdem> _resultadosProcessamento = new();

        public ArmazenamentoExposicao()
        {
            foreach (SimboloAtivo simbolo in Enum.GetValues<SimboloAtivo>())
            {
                _exposicoes.TryAdd(simbolo, 0m);
            }
        }

        public ResultadoProcessamentoOrdem ProcessarAtomicamente(Ordem ordem,
                                                                 Func<decimal, ResultadoProcessamentoOrdem> processar,
                                                                 Func<ResultadoProcessamentoOrdem> criarResultadoConflito,
                                                                 Action<bool> registrarDuplicidade)
        {
            lock (_bloqueio)
            {
                if (_ordensProcessadas.TryGetValue(ordem.Identificador, out Ordem? ordemProcessada))
                {
                    if (PossuiMesmosDadosFinanceiros(ordemProcessada,
                                                     ordem))
                    {
                        registrarDuplicidade(false);
                        return _resultadosProcessamento[ordem.Identificador];
                    }

                    registrarDuplicidade(true);
                    return criarResultadoConflito();
                }

                decimal exposicaoAtual = _exposicoes[ordem.Simbolo];
                ResultadoProcessamentoOrdem resultado = processar(exposicaoAtual);

                if (resultado.Aceita)
                {
                    _exposicoes[ordem.Simbolo] = resultado.ExposicaoProjetada;
                }

                _ordensProcessadas.TryAdd(ordem.Identificador, ordem);
                _resultadosProcessamento.TryAdd(ordem.Identificador, resultado);

                return resultado;
            }
        }

        private static bool PossuiMesmosDadosFinanceiros(Ordem ordemOriginal,
                                                         Ordem ordemRecebida)
        {
            return ordemOriginal.Simbolo == ordemRecebida.Simbolo
                && ordemOriginal.Lado == ordemRecebida.Lado
                && ordemOriginal.Quantidade == ordemRecebida.Quantidade
                && ordemOriginal.Preco == ordemRecebida.Preco;
        }
    }
}
