namespace OrderGenerator.Api.Application.DTOs
{
    public sealed class CriarOrdemResponse
    {
        public string IdentificadorOrdem { get; init; } = string.Empty;
        public string Simbolo { get; init; } = string.Empty;
        public bool Aceita { get; init; }
        public string? MotivoRejeicao { get; init; }
        public string? IdentificadorExecucao { get; init; }
        public string? IdentificadorOrdemServidor { get; init; }
    }
}
