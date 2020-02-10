using CarpoolAPI.AuthenticationHandler;
using CarpoolAPI.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;

namespace CarpoolAPI
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

            services.AddDbContext<CarpoolContext>(opt =>
               opt.UseSqlServer(Configuration.GetConnectionString("Carpool")));

            services.AddControllers();
            services.AddHttpClient();
            services.AddResponseCaching();

            services.AddAuthentication("Token")
                    //.AddJwtBearer()
                    //.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options => Configuration.Bind("Token", options))
                    .AddScheme<TokenOptions, TokenHandler>("Token", null); 
            services.AddAuthorization();
            //services.AddMvc(option => option.EnableEndpointRouting = false);


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.Use(async (context, next) =>
            {
                context.Response.GetTypedHeaders().CacheControl =
                    new Microsoft.Net.Http.Headers.CacheControlHeaderValue()
                    {
                        Public = true,
                        MaxAge = TimeSpan.FromSeconds(10),
                    };
                context.Response.Headers[Microsoft.Net.Http.Headers.HeaderNames.Vary] =
                    new string[] { "Accept-Encoding", "Token"};
                await next();
            });

            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {   
                endpoints.MapControllers();
            });
            //app.Use(async (HttpContext context, Func<Task> next) => {
            //    await next.Invoke(); //execute the request pipeline

            //    if (context.Response.StatusCode == StatusCodes.Status302Found && context.Response.Headers.TryGetValue("Location", out var redirect))
            //    {
            //        var v = redirect.ToString();
            //        if (v.StartsWith($"{context.Request.Scheme}://{context.Request.Host}/Account/Login"))
            //        {
            //            context.Response.Headers["Location"] = $"{context.Request.Scheme}://{context.Request.Host}{context.Request.Path}";
            //            context.Response.StatusCode = 401;
            //        }
            //    }
            //});
        }
    }
}
