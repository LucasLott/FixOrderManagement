using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using OrderGenerator.Api.Application.DTOs;

namespace OrderGenerator.Api.Infrastructure.Http
{
    public sealed class ValidarCriarOrdemRequestFilter : IAsyncActionFilter
    {
        private readonly IValidator<CriarOrdemRequest> _validador;

        public ValidarCriarOrdemRequestFilter(IValidator<CriarOrdemRequest> validador)
        {
            _validador = validador;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext contexto, ActionExecutionDelegate proximo)
        {
            if (!contexto.ActionArguments.TryGetValue("requisicao", out object? valor)
                || valor is not CriarOrdemRequest requisicao)
            {
                var erros = new Dictionary<string, string[]>
                {
                    [nameof(CriarOrdemRequest)] = ["A requisição da ordem é obrigatória."]
                };

                contexto.Result = new BadRequestObjectResult(new ValidationProblemDetails(erros));
                return;
            }

            var resultado = await _validador.ValidateAsync(requisicao, contexto.HttpContext.RequestAborted);
            
            if (!resultado.IsValid)
            {
                foreach (var falha in resultado.Errors)
                {
                    contexto.ModelState.AddModelError(falha.PropertyName, falha.ErrorMessage);
                }

                contexto.Result = new BadRequestObjectResult(new ValidationProblemDetails(contexto.ModelState));
                return;
            }

            await proximo();
        }
    }
}
