using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Task_2EF.DAL;
using Task_2EF.DAL.DataManager;
using Task_2EF.DAL.Entities;
using Task_2EF.DAL.Repository;

namespace Task_2EF
{
    internal class Startup
    {
        private readonly IConfiguration _configuration;
        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSwaggerGen();
            services.AddControllers();
            services.AddOptions();
            services.AddCors(configs =>
            {
                configs.AddPolicy(
                    "AllowOrigin",
                    options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()
                );
            });
            // 
            services.AddAutoMapper(typeof(Startup));

            // DAL
            services.AddScoped<IDataRepository<Employee>, EmployeeManager>();

            // Database
            services.AddDbContext<DbContext, ApplicationContext>(options =>
            {
                options.UseSqlServer(_configuration.GetConnectionString("EmployeeDB"));
            });
            // Identity
            services.AddIdentity<User, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationContext>()
                .AddDefaultTokenProviders();

            services.AddAuthentication(opt =>
            {
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(opt =>
            {
                opt.SaveToken = true;
                opt.RequireHttpsMetadata = false;
                opt.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuerSigningKey = bool.Parse(_configuration["JsonWebTokenKeys:ValidateIssuerSigningKey"]),
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JsonWebTokenKeys:IssuerSigninKey"])),
                    ValidateIssuer = bool.Parse(_configuration["JsonWebTokenKeys:ValidateIssuer"]),
                    ValidAudience = _configuration["JsonWebTokenKeys:ValidAudience"],
                    ValidIssuer = _configuration["JsonWebTokenKeys:ValidIssuer"],
                    ValidateAudience = bool.Parse(_configuration["JsonWebTokenKeys:ValidateAudience"]),
                    RequireExpirationTime = bool.Parse(_configuration["JsonWebTokenKeys:RequireExpirationTime"]),
                    ValidateLifetime = bool.Parse(_configuration["JsonWebTokenKeys:ValidateLifetime"])
                };
            });

            services.Configure<IdentityOptions>(opt =>
            {
                // Password
                opt.Password.RequiredLength = 7;
                opt.Password.RequireDigit = false;
                opt.Password.RequireUppercase = false;

                // Email
                opt.User.RequireUniqueEmail = true;
            });

            services.ConfigureApplicationCookie(options =>
            {
                options.LogoutPath = null;
                options.LoginPath = "/api/product";
                options.AccessDeniedPath = "/api/address/add";
            });
        }
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseCors(configs => configs.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

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