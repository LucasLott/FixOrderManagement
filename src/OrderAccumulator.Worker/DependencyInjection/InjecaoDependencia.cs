using Microsoft.Extensions.DependencyInjection;
using OrderAccumulator.Worker.Application.Services;

namespace OrderAccumulator.Worker.DependencyInjection
{
    public static class InjecaoDependencia
    {
        public static IServiceCollection AdicionarDependencias(this IServiceCollection servicos)
        {
            servicos.AddSingleton<CalculadoraExposicao>();

            return servicos;
        }
    }
}
