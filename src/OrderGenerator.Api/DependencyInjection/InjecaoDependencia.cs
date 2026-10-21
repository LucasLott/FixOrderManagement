using FluentValidation;
using OrderGenerator.Api.Application.Services;
using OrderGenerator.Api.Application.Validators;
using OrderGenerator.Api.Infrastructure.Fix;
using OrderGenerator.Api.Infrastructure.Http;

namespace OrderGenerator.Api.DependencyInjection
{
    public static class InjecaoDependencia
    {
        public static IServiceCollection AdicionarDependencias(this IServiceCollection servicos,
                                                               IConfiguration configuracao)
        {
            servicos.AddSingleton<GerenciadorSolicitacoesOrdens>();
            servicos.AddSingleton<ConversorOrdemFix>();
            servicos.AddSingleton<ConversorExecutionReport>();
            servicos.AddSingleton<ClienteFix>();
            servicos.AddSingleton<AplicacaoFix>(provedor => new AplicacaoFix(provedor.GetRequiredService<ConversorExecutionReport>(),
                                                                             provedor.GetRequiredService<GerenciadorSolicitacoesOrdens>(),
                                                                             provedor.GetRequiredService<ClienteFix>().DefinirSessaoAtiva,
                                                                             provedor.GetRequiredService<ILogger<AplicacaoFix>>()));
            servicos.AddSingleton<IHostedService>(provedor => provedor.GetRequiredService<ClienteFix>());
            servicos.AddSingleton<ServicoEnvioOrdens>();
            servicos.AddValidatorsFromAssemblyContaining<CriarOrdemRequestValidator>();
            servicos.AddScoped<ValidarCriarOrdemRequestFilter>();
            servicos.AddExceptionHandler<TratadorExcecoesApi>();
            servicos.AddProblemDetails();

            return servicos;
        }
    }
}
