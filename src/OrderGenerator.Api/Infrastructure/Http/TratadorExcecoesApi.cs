using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace OrderGenerator.Api.Infrastructure.Http
{
    public sealed class TratadorExcecoesApi : IExceptionHandler
    {
        private readonly ILogger<TratadorExcecoesApi> _logger;

        public TratadorExcecoesApi(ILogger<TratadorExcecoesApi> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext contexto,
                                                    Exception excecao,
                                                    CancellationToken cancellationToken)
        {
            if (excecao is OperationCanceledException && contexto.RequestAborted.IsCancellationRequested)
            {
                contexto.Response.StatusCode = StatusCodes.Status499ClientClosedRequest;
                return true;
            }

            _logger.LogError(excecao, "Erro inesperado ao processar requisição {Caminho}.", contexto.Request.Path);
            
            contexto.Response.StatusCode = StatusCodes.Status500InternalServerError;
            contexto.Response.ContentType = "application/problem+json";

            var problema = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Erro interno do servidor",
                Detail = "Ocorreu uma falha inesperada ao processar a solicitação.",
                Instance = contexto.Request.Path
            };

            await contexto.Response.WriteAsJsonAsync(problema, cancellationToken);
            return true;
        }
    }
}
