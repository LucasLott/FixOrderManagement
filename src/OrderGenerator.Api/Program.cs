using OrderGenerator.Api.DependencyInjection;

namespace OrderGenerator.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AdicionarDependencias(builder.Configuration);

            var app = builder.Build();

            app.Run();
        }
    }
}
