using System.Collections.Concurrent;
using OrderGenerator.Api.Application.Models;
using OrderGenerator.Api.Infrastructure.Fix;

namespace OrderGenerator.Api.Application.Services
{
    public sealed class GerenciadorSolicitacoesOrdens
    {
        private readonly ConcurrentDictionary<string, TaskCompletionSource<ResultadoEnvioOrdem>> _solicitacoesPendentes = new();
        private readonly ILogger<GerenciadorSolicitacoesOrdens> _logger;

        public GerenciadorSolicitacoesOrdens(ILogger<GerenciadorSolicitacoesOrdens> logger)
        {
            _logger = logger;
        }

        public async Task<ResultadoEnvioOrdem> EnviarAsync(SolicitacaoOrdem solicitacao,
                                                           TimeSpan timeout,
                                                           CancellationToken cancellationToken)
        {
            if (timeout <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(timeout), "O timeout deve ser positivo.");
            }

            string identificador = string.IsNullOrWhiteSpace(solicitacao.Identificador) ? Guid.NewGuid().ToString("N") : solicitacao.Identificador;

            var solicitacaoComIdentificador = new SolicitacaoOrdem
            {
                Identificador = identificador,
                Simbolo = solicitacao.Simbolo,
                Lado = solicitacao.Lado,
                Quantidade = solicitacao.Quantidade,
                Preco = solicitacao.Preco
            };

            var conclusao = new TaskCompletionSource<ResultadoEnvioOrdem>(TaskCreationOptions.RunContinuationsAsynchronously);

            if (!_solicitacoesPendentes.TryAdd(identificador, conclusao))
                throw new InvalidOperationException($"Já existe uma solicitação pendente com o identificador {identificador}.");

            try
            {
                var cancelamentoTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

                cancelamentoTimeout.CancelAfter(timeout);

                try
                {
                    var cliente = ClienteFix.InstanciaAtual;

                    if (cliente is null || !cliente.Enviar(solicitacaoComIdentificador))
                    {
                        _logger.LogWarning("Sessão FIX indisponível ou falha ao enviar a ordem {IdentificadorOrdem}.", identificador);
                        return CriarResultado(solicitacaoComIdentificador, EstadoEnvioOrdem.SessaoIndisponivel, "Sessão FIX indisponível ou falha no envio.");
                    }

                    _logger.LogInformation("NewOrderSingle enviada para a ordem {IdentificadorOrdem}.", identificador);
                    return await conclusao.Task.WaitAsync(cancelamentoTimeout.Token);
                }
                catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
                {
                    _logger.LogWarning("Timeout aguardando ExecutionReport para a ordem {IdentificadorOrdem}.", identificador);
                    return CriarResultado(solicitacaoComIdentificador, EstadoEnvioOrdem.Timeout, "Tempo limite aguardando resposta FIX.");
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Solicitação da ordem {IdentificadorOrdem} cancelada.", identificador);
                    return CriarResultado(solicitacaoComIdentificador, EstadoEnvioOrdem.Cancelada, "Solicitação cancelada.");
                }
            }
            catch (Exception excecao)
            {
                _logger.LogError(excecao, "Falha ao enviar a ordem {IdentificadorOrdem}.", identificador);
                return CriarResultado(solicitacaoComIdentificador, EstadoEnvioOrdem.FalhaEnvio, "Falha ao enviar a ordem FIX.");
            }
            finally
            {
                _solicitacoesPendentes.TryRemove(identificador, out _);
            }
        }

        public bool Concluir(ResultadoEnvioOrdem resultado)
        {
            if (_solicitacoesPendentes.TryGetValue(resultado.IdentificadorOrdem, out var conclusao))
                return conclusao.TrySetResult(resultado);

            _logger.LogWarning("ExecutionReport recebido sem solicitação pendente para {IdentificadorOrdem}.", resultado.IdentificadorOrdem);
            return false;
        }

        private static ResultadoEnvioOrdem CriarResultado(SolicitacaoOrdem solicitacao,
                                                          EstadoEnvioOrdem estado,
                                                          string motivo)
        {
            return new ResultadoEnvioOrdem
            {
                IdentificadorOrdem = solicitacao.Identificador,
                Simbolo = solicitacao.Simbolo,
                Estado = estado,
                MotivoRejeicao = motivo
            };
        }
    }
}
