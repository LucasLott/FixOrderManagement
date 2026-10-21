using Microsoft.Extensions.Logging.Abstractions;
using OrderAccumulator.Worker.Application.Models;
using OrderAccumulator.Worker.Application.Services;
using OrderAccumulator.Worker.Domain.Entities;
using OrderAccumulator.Worker.Domain.Enums;
using OrderAccumulator.Worker.Infrastructure.Memory;

namespace OrderAccumulator.Tests
{
    public class ProcessadorOrdensTests
    {
        private readonly ProcessadorOrdens _processador = new(new CalculadoraExposicao(), new ArmazenamentoExposicao(), NullLogger<ProcessadorOrdens>.Instance);

        private static Ordem CriarOrdem(string identificador,
                                        SimboloAtivo simbolo = SimboloAtivo.PETR4,
                                        LadoOrdem lado = LadoOrdem.Compra,
                                        int quantidade = 1,
                                        decimal preco = 0.01m)
        {
            return new Ordem(identificador, simbolo, lado, quantidade, preco);
        }

        private ResultadoProcessamentoOrdem Processar(SimboloAtivo simbolo,
                                                      LadoOrdem lado,
                                                      int quantidade,
                                                      decimal preco)
        {
            return _processador.ProcessarOrdem(CriarOrdem(Guid.NewGuid().ToString(),
                                                          simbolo,
                                                          lado,
                                                          quantidade,
                                                          preco));
        }

        public static TheoryData<decimal, bool> Limites => new()
        {
            { 0m, true }, { 99_999_999.99m, true }, { 100_000_000m, true }, { 100_000_000.01m, false },
            { -99_999_999.99m, true }, { -100_000_000m, true }, { -100_000_000.01m, false }
        };

        [Theory]
        [MemberData(nameof(Limites))]
        public void ProcessarOrdem_ExposicaoNosLimites_DeveAplicarLimiteAbsoluto(decimal projetada, bool aceita)
        {
            if (projetada == 0m)
            {
                Processar(SimboloAtivo.PETR4, LadoOrdem.Compra, 1, 0.01m);
                var resultadoZero = Processar(SimboloAtivo.PETR4, LadoOrdem.Venda, 1, 0.01m);
                Assert.True(resultadoZero.Aceita);
                Assert.Equal(0m, resultadoZero.ExposicaoProjetada);
                return;
            }

            LadoOrdem lado = projetada > 0 ? LadoOrdem.Compra : LadoOrdem.Venda;
            Assert.True(Processar(SimboloAtivo.PETR4, lado, 99_999, 999.99m).Aceita);
            var segunda = Processar(SimboloAtivo.PETR4, lado, 2, 999.99m);
            Assert.True(segunda.Aceita);
            ResultadoProcessamentoOrdem resultado = Math.Abs(projetada) == 99_999_999.99m
                ? segunda
                : Processar(SimboloAtivo.PETR4, lado, Math.Abs(projetada) == 100_000_000m ? 1 : 2, 0.01m);
            Assert.Equal(projetada, resultado.ExposicaoProjetada);
            Assert.Equal(aceita, resultado.Aceita);
            if (!aceita)
            {
                Assert.Equal(lado == LadoOrdem.Compra ? 99_999_999.99m : -99_999_999.99m, resultado.ExposicaoAtual);
            }
        }

        [Fact]
        public void ProcessarOrdem_SequenciaManual_DeveAceitarAteLimiteERejeitarExcesso()
        {
            Assert.Equal(99_998_000.01m, Processar(SimboloAtivo.PETR4, LadoOrdem.Compra, 99_999, 999.99m).ExposicaoAtual);
            var segunda = Processar(SimboloAtivo.PETR4, LadoOrdem.Compra, 2, 999.99m);
            var terceira = Processar(SimboloAtivo.PETR4, LadoOrdem.Compra, 1, 0.01m);
            var quarta = Processar(SimboloAtivo.PETR4, LadoOrdem.Compra, 1, 0.01m);
            Assert.True(segunda.Aceita);
            Assert.Equal(99_999_999.99m, segunda.ExposicaoAtual);
            Assert.True(terceira.Aceita);
            Assert.Equal(100_000_000m, terceira.ExposicaoAtual);
            Assert.False(quarta.Aceita);
            Assert.Equal(100_000_000m, quarta.ExposicaoAtual);
        }

        [Fact]
        public void ProcessarOrdem_SimbolosDiferentes_DeveManterExposicoesIndependentes()
        {
            Processar(SimboloAtivo.PETR4, LadoOrdem.Compra, 99_999, 999.99m);
            Processar(SimboloAtivo.PETR4, LadoOrdem.Compra, 2, 999.99m);
            Processar(SimboloAtivo.PETR4, LadoOrdem.Compra, 1, 0.01m);
            Assert.Equal(-50m, Processar(SimboloAtivo.VIIA4, LadoOrdem.Venda, 5, 10m).ExposicaoAtual);
            Assert.Equal(100m, Processar(SimboloAtivo.VALE3, LadoOrdem.Compra, 10, 10m).ExposicaoAtual);
            Assert.Equal(99_999_999.99m, Processar(SimboloAtivo.PETR4, LadoOrdem.Venda, 1, 0.01m).ExposicaoAtual);
            Assert.Equal(-40m, Processar(SimboloAtivo.VIIA4, LadoOrdem.Compra, 1, 10m).ExposicaoAtual);
        }

        [Fact]
        public void ProcessarOrdem_MesmoIdentificadorEDados_DeveRetornarResultadoAnteriorSemSomar()
        {
            var ordem = CriarOrdem("igual", quantidade: 10, preco: 10m);
            var primeiro = _processador.ProcessarOrdem(ordem);
            var segundo = _processador.ProcessarOrdem(CriarOrdem("igual", quantidade: 10, preco: 10m));
            Assert.Same(primeiro, segundo);
            Assert.Equal(100.01m, Processar(SimboloAtivo.PETR4, LadoOrdem.Compra, 1, 0.01m).ExposicaoAtual);
        }

        [Theory]
        [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)]
        public void ProcessarOrdem_MesmoIdentificadorComDadosDiferentes_DeveRejeitarConflito(int alteracao)
        {
            _processador.ProcessarOrdem(CriarOrdem("conflito", quantidade: 10, preco: 10m));
            var diferente = CriarOrdem("conflito", alteracao == 0 ? SimboloAtivo.VALE3 : SimboloAtivo.PETR4,
                alteracao == 1 ? LadoOrdem.Venda : LadoOrdem.Compra, alteracao == 2 ? 11 : 10, alteracao == 3 ? 11m : 10m);
            var resultado = _processador.ProcessarOrdem(diferente);
            Assert.False(resultado.Aceita);
            Assert.Contains("identificador", resultado.MotivoRejeicao, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(100.01m, Processar(SimboloAtivo.PETR4, LadoOrdem.Compra, 1, 0.01m).ExposicaoAtual);
            Assert.Equal(0.01m, Processar(SimboloAtivo.VALE3, LadoOrdem.Compra, 1, 0.01m).ExposicaoAtual);
        }

        [Fact]
        public void ProcessarOrdem_IdentificadoresDiferentes_DeveProcessarCadaOrdem()
        {
            Assert.Equal(100m, _processador.ProcessarOrdem(CriarOrdem("1", quantidade: 10, preco: 10m)).ExposicaoAtual);
            Assert.Equal(200m, _processador.ProcessarOrdem(CriarOrdem("2", quantidade: 10, preco: 10m)).ExposicaoAtual);
        }

        [Fact]
        public void ProcessarOrdem_RepeticaoDeRejeitada_DeveRetornarRejeicaoOriginal()
        {
            Processar(SimboloAtivo.PETR4, LadoOrdem.Compra, 99_999, 999.99m);
            Processar(SimboloAtivo.PETR4, LadoOrdem.Compra, 2, 999.99m);
            var ordem = CriarOrdem("rejeitada", quantidade: 2);
            var primeiro = _processador.ProcessarOrdem(ordem);
            Assert.False(primeiro.Aceita);
            Processar(SimboloAtivo.PETR4, LadoOrdem.Venda, 1, 1m);
            Assert.Same(primeiro, _processador.ProcessarOrdem(ordem));
            Assert.Equal(99_999_999m, Processar(SimboloAtivo.PETR4, LadoOrdem.Compra, 1, 0.01m).ExposicaoAtual);
        }

        [Fact]
        public async Task ProcessarOrdem_ComprasConcorrentes_DeveRespeitarLimiteAsync()
        {
            var tarefas = Enumerable.Range(0, 20).Select(indice => Task.Run(() => _processador.ProcessarOrdem(CriarOrdem(indice.ToString(), quantidade: 20_000, preco: 500m))));
            var resultados = await Task.WhenAll(tarefas);
            Assert.Equal(10, resultados.Count(resultado => resultado.Aceita));
            Assert.Equal(10, resultados.Count(resultado => !resultado.Aceita));
            var verificacao = Processar(SimboloAtivo.PETR4, LadoOrdem.Compra, 1, 0.01m);
            Assert.False(verificacao.Aceita);
            Assert.Equal(100_000_000m, verificacao.ExposicaoAtual);
        }

        [Fact]
        public async Task ProcessarOrdem_MesmoIdentificadorConcorrente_DeveContabilizarUmaVezAsync()
        {
            var tarefas = Enumerable.Range(0, 20).Select(_ => Task.Run(() => _processador.ProcessarOrdem(CriarOrdem("mesmo", quantidade: 20_000, preco: 500m))));
            var resultados = await Task.WhenAll(tarefas);
            Assert.All(resultados, resultado => Assert.Same(resultados[0], resultado));
            Assert.True(resultados[0].Aceita);
            Assert.Equal(10_000_000.01m, Processar(SimboloAtivo.PETR4, LadoOrdem.Compra, 1, 0.01m).ExposicaoAtual);
        }
    }
}

