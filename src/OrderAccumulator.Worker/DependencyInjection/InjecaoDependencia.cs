using Microsoft.Extensions.DependencyInjection;
using OrderAccumulator.Worker.Application.Services;
using OrderAccumulator.Worker.Infrastructure.Memory;

namespace OrderAccumulator.Worker.DependencyInjection
{
    public static class InjecaoDependencia
    {
        public static IServiceCollection AdicionarDependencias(this IServiceCollection servicos)
        {
            servicos.AddSingleton<CalculadoraExposicao>();
            servicos.AddSingleton<ArmazenamentoExposicao>();
            servicos.AddSingleton<ProcessadorOrdens>();

            return servicos;
        }
    }
}
