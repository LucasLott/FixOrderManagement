using OrderGenerator.Api.Application.DTOs;
using OrderGenerator.Api.Application.Validators;

namespace OrderGenerator.Tests
{
    public class CriarOrdemRequestValidatorTests
    {
        private readonly CriarOrdemRequestValidator _validador = new();

        private void Verificar(string simbolo,
                               string lado,
                               int quantidade,
                               decimal preco,
                               string? propriedadeInvalida)
        {
            var resultado = _validador.Validate(new CriarOrdemRequest { Simbolo = simbolo, Lado = lado, Quantidade = quantidade, Preco = preco });
            if (propriedadeInvalida is null)
            {
                Assert.True(resultado.IsValid);
            }
            else
            {
                Assert.Contains(resultado.Errors, erro => erro.PropertyName == propriedadeInvalida);
            }
        }

        [Theory]
        [InlineData("PETR4")] [InlineData("VALE3")] [InlineData("VIIA4")]
        public void Validar_SimboloPermitido_DeveAceitar(string simbolo) => Verificar(simbolo, "Compra", 1, 1m, null);

        [Theory]
        [InlineData("INVALIDO")] [InlineData("")]
        public void Validar_SimboloInvalido_DeveRejeitar(string simbolo) => Verificar(simbolo, "Compra", 1, 1m, nameof(CriarOrdemRequest.Simbolo));

        [Theory]
        [InlineData("Compra")] [InlineData("Venda")]
        public void Validar_LadoPermitido_DeveAceitar(string lado) => Verificar("PETR4", lado, 1, 1m, null);

        [Theory]
        [InlineData("Invalido")] [InlineData("")]
        public void Validar_LadoInvalido_DeveRejeitar(string lado) => Verificar("PETR4", lado, 1, 1m, nameof(CriarOrdemRequest.Lado));

        [Theory]
        [InlineData(0)] [InlineData(-1)] [InlineData(100_000)]
        public void Validar_QuantidadeInvalida_DeveRejeitar(int quantidade) => Verificar("PETR4", "Compra", quantidade, 1m, nameof(CriarOrdemRequest.Quantidade));

        public static TheoryData<decimal> PrecosInvalidos => new() { 0m, -1m, 1000m, 1000.01m, 1.001m };

        [Theory]
        [MemberData(nameof(PrecosInvalidos))]
        public void Validar_PrecoInvalido_DeveRejeitar(decimal preco) => Verificar("PETR4", "Compra", 1, preco, nameof(CriarOrdemRequest.Preco));

        [Fact]
        public void Validar_RequisicaoNosLimitesPermitidos_DeveAceitar() => Verificar("VIIA4", "Venda", 99_999, 999.99m, null);
    }
}
