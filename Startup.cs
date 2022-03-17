using ExamSaver.Configs;
using ExamSaver.Data;
using ExamSaver.Models;
using ExamSaver.Services;
using ExamSaver.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace ExamSaver
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
            services.AddControllers(options =>
            {
                options.UseGeneralRoutePrefix(Constant.URL_PREFIX);
            });

            IConfigurationSection appSettingsSection = Configuration.GetSection("AppSettings");

            services.Configure<AppSettings>(appSettingsSection);

            AppSettings appSettings = appSettingsSection.Get<AppSettings>();

            services.AddAuthentication(configureOptions =>
            {
                configureOptions.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                configureOptions.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(configureOptions =>
            {
                configureOptions.RequireHttpsMetadata = false;
                configureOptions.SaveToken = true;
                configureOptions.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(appSettings.JWTSecretKey)),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });

            services.AddDbContext<DatabaseContext>(ServiceLifetime.Scoped);

            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IExamService, ExamService>();
            services.AddScoped<IFileService, FileService>();
            services.AddScoped<ISubjectService, SubjectService>();
            services.AddScoped<ISimilarityService, SimilarityService>();
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

            app.UseCors(options =>
                options
                .SetIsOriginAllowed(origin => false)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .WithExposedHeaders("Content-Disposition")
                .WithExposedHeaders("X-Pagination")
            );

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseMiddleware<ExceptionHandlerMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
