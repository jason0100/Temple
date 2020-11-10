using api.Helpers;
using API.Data;
using API.Helpers;
using API.Models.Users;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Threading.Tasks;

namespace API
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
           
            services.AddDbContext<TempleDbContext>(options =>
            options.UseSqlite(Configuration["DBConnectionString"]));
            //options.UseSqlite("Data Source=../Temple.db"));

            services.AddControllers();
            services.AddScoped<ITokenGetUserHelper, TokenGetUserHelper>();
            services.AddScoped<IAuthRepository, AuthRepository>();

            //services.AddScoped<IEmailSender, EmailSender>();
            services.AddTransient<IEmailSender, EmailSender>();
            services.AddTransient<IUploadHelper, UploadHelper>();
            //services.Configure<AuthMessageSenderOptions>(Configuration);
            //=====================================

            var key = Encoding.ASCII.GetBytes(Configuration["TokenKey"]);
            
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                   
                    ValidateLifetime = true, // 驗證 Token 的有效期間
                };
                
            });
            //=====================================
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
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
