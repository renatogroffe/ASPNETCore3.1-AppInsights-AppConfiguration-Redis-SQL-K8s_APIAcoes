using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using APIAcoes.Data;

namespace APIAcoes
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddEntityFrameworkSqlServer()
                .AddDbContext<AcoesContext>(
                    options => options.UseSqlServer(
                        Configuration["BaseAcoesEF"]));

            services.AddControllers();
            
            services.AddSwaggerGen(c => {

                c.SwaggerDoc("v1",
                    new OpenApiInfo
                    {
                        Title = "Ações - Testes - Azure Queue Storage",
                        Version = "v1",
                        Description = $"Instância: {Environment.MachineName} | Exemplo de API REST criada com o ASP.NET Core 3.1 para o envio de dados de ações a uma Fila do Azure Queue Storage",
                        Contact = new OpenApiContact
                        {
                            Name = "Renato Groffe",
                            Url = new Uri("https://github.com/renatogroffe")
                        }
                    });
            });

            services.AddApplicationInsightsTelemetry(Configuration);
            services.ConfigureTelemetryModule<DependencyTrackingTelemetryModule>(
                (module, o) =>
                {
                    module. EnableSqlCommandTextInstrumentation = true;
                });

            services.AddAzureAppConfiguration();

            services.AddSingleton<ConnectionMultiplexer>(
                ConnectionMultiplexer.Connect(
                    Configuration["Redis:Connection"]));
            services.AddScoped<AcoesRepository>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            
            app.UseAzureAppConfiguration();

            app.UseCors(builder => builder.AllowAnyMethod()
                                          .AllowAnyOrigin()
                                          .AllowAnyHeader());
                                          
            app.UseSwagger();
            app.UseSwaggerUI(c => {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Ações - Testes - Azure Queue Storage");
                c.RoutePrefix = string.Empty;
            });

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}