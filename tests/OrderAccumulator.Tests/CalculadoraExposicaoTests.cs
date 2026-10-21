using OrderAccumulator.Worker.Application.Services;
using OrderAccumulator.Worker.Domain.Entities;
using OrderAccumulator.Worker.Domain.Enums;

namespace OrderAccumulator.Tests
{
    public class CalculadoraExposicaoTests
    {
        public static TheoryData<int, decimal, decimal> Valores => new()
        {
            { 3, 0.10m, 0.30m },
            { 99_999, 999.99m, 99_998_000.01m },
            { 1, 0.01m, 0.01m }
        };

        [Theory]
        [MemberData(nameof(Valores))]
        public void CalcularValorOrdem_QuantidadeEPreco_DeveMultiplicarComPrecisao(int quantidade,
                                                                                   decimal preco,
                                                                                   decimal esperado)
        {
            var ordem = new Ordem("1", SimboloAtivo.PETR4, LadoOrdem.Compra, quantidade, preco);
            Assert.Equal(esperado, new CalculadoraExposicao().CalcularValorOrdem(ordem));
        }

        [Theory]
        [InlineData(LadoOrdem.Compra, 100, 130)]
        [InlineData(LadoOrdem.Venda, 100, 70)]
        [InlineData(LadoOrdem.Venda, 0, -30)]
        [InlineData(LadoOrdem.Compra, -100, -70)]
        public void CalcularExposicaoProjetada_LadoDaOrdem_DeveAplicarValor(LadoOrdem lado,
                                                                            int atual,
                                                                            int esperado)
        {
            var ordem = new Ordem("1", SimboloAtivo.PETR4, lado, 3, 10m);
            Assert.Equal((decimal)esperado, new CalculadoraExposicao().CalcularExposicaoProjetada(atual, ordem));
        }
    }
}
