namespace OrderGenerator.Api.Application.Models
{
    public enum EstadoEnvioOrdem
    {
        Aceita,
        Rejeitada,
        EstadoInesperado,
        SessaoIndisponivel,
        FalhaEnvio,
        Timeout,
        Cancelada
    }

    public sealed class ResultadoEnvioOrdem
    {
        public string IdentificadorOrdem { get; init; } = string.Empty;
        public string IdentificadorExecucao { get; init; } = string.Empty;
        public string IdentificadorOrdemServidor { get; init; } = string.Empty;
        public SimboloOrdem Simbolo { get; init; }
        public EstadoEnvioOrdem Estado { get; init; }
        public bool Aceita => Estado == EstadoEnvioOrdem.Aceita;
        public string? MotivoRejeicao { get; init; }
    }
}
