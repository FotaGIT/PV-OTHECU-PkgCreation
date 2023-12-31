using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PkgCreation.Repository.Interface;
using PkgCreation.Repository.RepositoryClass;

namespace PkgCreation
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(name: "AllowOrigin",
                builder =>
                {
                    builder.AllowAnyOrigin().SetIsOriginAllowedToAllowWildcardSubdomains()
                                        .AllowAnyHeader()
                                        .AllowAnyMethod();
                });
            });
            services.AddControllers();
           

            services.AddScoped<IPacakgeRepository, PackageRepository>();
            services.AddScoped<IManualPlanRepository, ManualPlanRepository>();
            services.AddScoped<IExceptionErrorLog, ExceptionErrorLog>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseCors("AllowOrigin");

            app.UseAuthorization();

            //app.UseCors(options => options.WithOrigins("http://localhost:4200", "https://cvpfotaportal-qa.tatamotors.com").AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers().RequireCors("AllowOrigin");
            });
        }
    }
}
