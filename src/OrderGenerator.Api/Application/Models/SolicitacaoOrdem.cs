namespace OrderGenerator.Api.Application.Models
{
    public enum SimboloOrdem
    {
        PETR4,
        VALE3,
        VIIA4
    }

    public enum LadoSolicitacaoOrdem
    {
        Compra,
        Venda
    }

    public sealed class SolicitacaoOrdem
    {
        public string Identificador { get; init; } = string.Empty;
        public SimboloOrdem Simbolo { get; init; }
        public LadoSolicitacaoOrdem Lado { get; init; }
        public int Quantidade { get; init; }
        public decimal Preco { get; init; }
    }
}
