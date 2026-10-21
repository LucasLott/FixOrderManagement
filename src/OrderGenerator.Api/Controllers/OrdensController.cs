using Microsoft.AspNetCore.Mvc;
using OrderGenerator.Api.Application.DTOs;
using OrderGenerator.Api.Application.Models;
using OrderGenerator.Api.Application.Services;
using OrderGenerator.Api.Infrastructure.Http;

namespace OrderGenerator.Api.Controllers
{
    /// <summary>
    /// Recebe solicitações HTTP para envio de ordens ao Accumulator.
    /// </summary>
    [ApiController]
    [Route("api/ordens")]
    [Produces("application/json")]
    public sealed class OrdensController : ControllerBase
    {
        private readonly ServicoEnvioOrdens _servico;
        private readonly ILogger<OrdensController> _logger;

        public OrdensController(ServicoEnvioOrdens servico,
                                ILogger<OrdensController> logger)
        {
            _servico = servico;
            _logger = logger;
        }

        /// <summary>
        /// Valida e envia uma ordem de compra ou venda para processamento.
        /// </summary>
        /// <param name="requisicao">Símbolo, lado, quantidade e preço da ordem.</param>
        /// <param name="cancellationToken">Token para cancelar a solicitação HTTP.</param>
        /// <returns>O resultado do Accumulator ou um problema HTTP quando o processamento técnico não for confirmado.</returns>
        /// <remarks>
        /// Aceita PETR4, VALE3 ou VIIA4 e os lados Compra ou Venda. A quantidade deve ser maior que zero e menor que 100.000. O preço deve ser maior que zero, menor que 1.000 e múltiplo de R$ 0,01.
        /// Exemplo: { "simbolo": "PETR4", "lado": "Compra", "quantidade": 100, "preco": 35.50 }.
        /// Ordens aceitas ou rejeitadas por regras de negócio retornam HTTP 200. Entradas inválidas retornam 400, sessão FIX indisponível retorna 503 e timeout retorna 504.
        /// </remarks>
        [HttpPost]
        [ServiceFilter(typeof(ValidarCriarOrdemRequestFilter))]
        [ProducesResponseType(typeof(CriarOrdemResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status499ClientClosedRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status504GatewayTimeout)]
        public async Task<IActionResult> CriarOrdemAsync([FromBody] CriarOrdemRequest requisicao,
                                                         CancellationToken cancellationToken)
        {
            SimboloOrdem simbolo = Enum.Parse<SimboloOrdem>(requisicao.Simbolo);
            LadoSolicitacaoOrdem lado = Enum.Parse<LadoSolicitacaoOrdem>(requisicao.Lado);

            string identificador = Guid.NewGuid().ToString("N");
            
            _logger.LogInformation("Solicitação HTTP recebida para a ordem {IdentificadorOrdem}.", identificador);

            var solicitacao = new SolicitacaoOrdem
            {
                Identificador = identificador,
                Simbolo = simbolo,
                Lado = lado,
                Quantidade = requisicao.Quantidade,
                Preco = requisicao.Preco
            };

            ResultadoEnvioOrdem resultado = await _servico.EnviarOrdemAsync(solicitacao,
                                                                            cancellationToken);

            if (resultado.Estado is EstadoEnvioOrdem.Aceita or EstadoEnvioOrdem.Rejeitada)
            {
                return Ok(new CriarOrdemResponse
                {
                    IdentificadorOrdem = resultado.IdentificadorOrdem,
                    Simbolo = resultado.Simbolo.ToString(),
                    Aceita = resultado.Aceita,
                    MotivoRejeicao = resultado.MotivoRejeicao,
                    IdentificadorExecucao = resultado.IdentificadorExecucao,
                    IdentificadorOrdemServidor = resultado.IdentificadorOrdemServidor
                });
            }

            int codigoStatus = resultado.Estado switch
            {
                EstadoEnvioOrdem.SessaoIndisponivel => StatusCodes.Status503ServiceUnavailable,
                EstadoEnvioOrdem.Timeout => StatusCodes.Status504GatewayTimeout,
                EstadoEnvioOrdem.Cancelada => StatusCodes.Status499ClientClosedRequest,
                _ => StatusCodes.Status500InternalServerError
            };

            var problema = new ProblemDetails
            {
                Status = codigoStatus,
                Title = resultado.Estado switch
                {
                    EstadoEnvioOrdem.SessaoIndisponivel => "Sessão FIX indisponível",
                    EstadoEnvioOrdem.Timeout => "Resultado da ordem não confirmado",
                    EstadoEnvioOrdem.Cancelada => "Solicitação cancelada",
                    _ => "Falha ao processar a ordem"
                },
                Detail = resultado.MotivoRejeicao,
                Instance = HttpContext.Request.Path
            };

            problema.Extensions["identificadorOrdem"] = identificador;

            return StatusCode(codigoStatus, problema);
        }
    }
}
