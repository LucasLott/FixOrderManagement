using OrderAccumulator.Worker.DependencyInjection;

namespace OrderAccumulator.Worker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);
            builder.Services.AdicionarDependencias();

            var host = builder.Build();
            host.Run();
        }
    }
}
