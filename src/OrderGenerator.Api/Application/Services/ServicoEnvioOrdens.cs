using OrderGenerator.Api.Application.Models;
using OrderGenerator.Api.Infrastructure.Fix;

namespace OrderGenerator.Api.Application.Services
{
    public sealed class ServicoEnvioOrdens
    {
        private readonly GerenciadorSolicitacoesOrdens _gerenciador;
        private readonly IConfiguration _configuracao;
        private readonly ILogger<ServicoEnvioOrdens> _logger;

        public ServicoEnvioOrdens(GerenciadorSolicitacoesOrdens gerenciador,
                                  IConfiguration configuracao,
                                  ILogger<ServicoEnvioOrdens> logger)
        {
            _gerenciador = gerenciador;
            _configuracao = configuracao;
            _logger = logger;
        }

        public Task<ResultadoEnvioOrdem> EnviarOrdemAsync(SolicitacaoOrdem solicitacao,
                                                          CancellationToken cancellationToken = default)
        {
            string? erro = Validar(solicitacao);

            if (erro is not null)
                throw new ArgumentException(erro, nameof(solicitacao));

            TimeSpan timeout = TimeSpan.FromSeconds(_configuracao.GetValue<int?>("Fix:TimeoutSegundos") ?? 30);

            _logger.LogInformation("Iniciando envio da ordem {IdentificadorOrdem}.", solicitacao.Identificador);
            return _gerenciador.EnviarAsync(solicitacao, timeout, cancellationToken);
        }

        private static string? Validar(SolicitacaoOrdem solicitacao)
        {
            if (solicitacao is null)
                return "A solicitação da ordem é obrigatória.";

            if (!Enum.IsDefined(solicitacao.Simbolo))
                return "Símbolo da ordem não permitido.";

            if (!Enum.IsDefined(solicitacao.Lado))
                return "Lado da ordem não permitido.";

            if (solicitacao.Quantidade <= 0 || solicitacao.Quantidade >= 100_000)
                return "A quantidade deve ser positiva e menor que 100.000.";

            if (solicitacao.Preco <= 0 || solicitacao.Preco >= 1_000 || solicitacao.Preco != decimal.Round(solicitacao.Preco, 2))
                return "O preço deve ser positivo, menor que 1.000 e possuir incrementos de R$ 0,01.";

            return null;
        }
    }
}
