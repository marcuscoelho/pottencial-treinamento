using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Pottencial.Invoices.Repositories.Invoices.Context;
using Pottencial.Invoices.Repositories.Invoices.Repositories;
using Pottencial.Invoices.UseCases.Invoices.UseCases;

namespace Pottencial.Invoices.Api
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
            services.AddControllers();

            services
                .AddScoped<InvoiceUseCases>()
                .AddScoped<CancelInvoiceUseCase>()
                .AddScoped<ChangeInvoiceUseCase>()
                .AddScoped<CreateInvoiceUseCase>()
                .AddScoped<GetInvoiceByNumberUseCase>()
                .AddScoped<GetInvoicesUseCase>()
                .AddScoped<SubmitInvoiceUseCase>();

            services
                .AddScoped<InvoiceRepository>()
                .AddDbContext<InvoiceDbContext>(options => options.UseInMemoryDatabase("Invoices"));

            services
                .AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("invoices", new OpenApiInfo { Title = "Invoices" });
                    c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "Pottencial.Invoices.Api.xml"), true);
                });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseSwagger()
               .UseSwaggerUI(c =>
               {
                   c.RoutePrefix = "api-docs";
                   c.SwaggerEndpoint($"../swagger/invoices/swagger.json", $"Invoices");
               });

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
