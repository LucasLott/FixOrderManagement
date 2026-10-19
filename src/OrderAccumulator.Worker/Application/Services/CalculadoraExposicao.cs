using OrderAccumulator.Worker.Domain.Constants;
using OrderAccumulator.Worker.Domain.Entities;
using OrderAccumulator.Worker.Domain.Enums;

namespace OrderAccumulator.Worker.Application.Services
{
    public class CalculadoraExposicao
    {
        public decimal CalcularValorOrdem(Ordem ordem)
        {
            ArgumentNullException.ThrowIfNull(ordem);

            return ordem.Preco * ordem.Quantidade;
        }

        public decimal CalcularExposicaoProjetada(decimal exposicaoAtual,
                                                  Ordem ordem)
        {
            decimal valorOrdem = CalcularValorOrdem(ordem);

            return ordem.Lado == LadoOrdem.Compra ? exposicaoAtual + valorOrdem : exposicaoAtual - valorOrdem;
        }

        public bool EstaDentroDoLimite(decimal exposicaoProjetada)
        {
            return exposicaoProjetada >= -LimitesFinanceiros.LimiteExposicaoPorSimbolo && exposicaoProjetada <= LimitesFinanceiros.LimiteExposicaoPorSimbolo;
        }
    }
}
