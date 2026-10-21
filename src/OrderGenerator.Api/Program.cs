using OrderGenerator.Api.DependencyInjection;
using Microsoft.OpenApi;

namespace OrderGenerator.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddControllers(opcoes =>
            {
                opcoes.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
            });
            builder.Services.AddSwaggerGen(opcoes =>
            {
                opcoes.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "FixOrderManagement API",
                    Version = "v1",
                    Description = "API para envio e processamento de ordens financeiras utilizando o protocolo FIX 4.4."
                });

                string caminhoDocumentacaoXml = Path.Combine(
                    AppContext.BaseDirectory,
                    $"{typeof(Program).Assembly.GetName().Name}.xml");
                opcoes.IncludeXmlComments(caminhoDocumentacaoXml);
            });
            string[] origensPermitidas = builder.Configuration
                .GetSection("Cors:OrigensPermitidas")
                .Get<string[]>() ?? [];
            builder.Services.AddCors(opcoes =>
            {
                opcoes.AddPolicy("Frontend", politica =>
                {
                    politica.WithOrigins(origensPermitidas)
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });
            builder.Services.AdicionarDependencias(builder.Configuration);

            var app = builder.Build();
            app.UseExceptionHandler();
            app.UseCors("Frontend");
            app.MapControllers();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(opcoes =>
                {
                    opcoes.RoutePrefix = "swagger";
                    opcoes.SwaggerEndpoint("/swagger/v1/swagger.json", "FixOrderManagement API v1");
                });
            }

            app.Run();
        }
    }
}
