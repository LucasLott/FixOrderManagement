namespace OrderGenerator.Api.Application.DTOs
{
    public sealed class CriarOrdemRequest
    {
        public string Simbolo { get; init; } = string.Empty;

        public string Lado { get; init; } = string.Empty;

        public int Quantidade { get; init; }

        public decimal Preco { get; init; }
    }
}
