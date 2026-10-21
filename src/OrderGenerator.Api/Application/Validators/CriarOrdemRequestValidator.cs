using FluentValidation;
using OrderGenerator.Api.Application.DTOs;

namespace OrderGenerator.Api.Application.Validators
{
    public sealed class CriarOrdemRequestValidator : AbstractValidator<CriarOrdemRequest>
    {
        public CriarOrdemRequestValidator()
        {
            RuleFor(requisicao => requisicao.Simbolo)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("O símbolo é obrigatório.")
                .Must(simbolo => simbolo is "PETR4" or "VALE3" or "VIIA4")
                .WithMessage("O símbolo informado é inválido.");

            RuleFor(requisicao => requisicao.Lado)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("O lado da ordem é obrigatório.")
                .Must(lado => lado is "Compra" or "Venda")
                .WithMessage("O lado da ordem deve ser Compra ou Venda.");

            RuleFor(requisicao => requisicao.Quantidade)
                .Must(quantidade => quantidade > 0 && quantidade < 100_000)
                .WithMessage("A quantidade deve ser maior que zero e menor que 100.000.");

            RuleFor(requisicao => requisicao.Preco)
                .Must(preco => preco > 0m && preco < 1_000m)
                .WithMessage("O preço deve ser maior que zero e menor que 1.000.")
                .Must(preco => preco % 0.01m == 0m)
                .WithMessage("O preço deve ser múltiplo de R$ 0,01.");
        }
    }
}
