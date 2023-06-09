﻿using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Task_2EF.AppServices;
using Task_2EF.Configuration;
using Task_2EF.Controllers;
using Task_2EF.DAL;
using Task_2EF.DAL.DataManager;
using Task_2EF.DAL.Entities;
using Task_2EF.DAL.Repository;
using Task_2EF.DAL.Services;

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
            services.AddControllers(option =>
            {
            });
            services.AddOptions();
            services.AddCors(configs =>
            {
                configs.AddPolicy(
                    "AllowOrigin",
                    options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()
                );
            });
            // 
            //services.AddAutoMapper(typeof(Startup));
            services.AddAutoMapper(configAction =>
            {
                configAction.AddProfile<MappingProfile>();
            });

            // DAL
            services.AddScoped<IService<Employee>, EmployeeService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddMemoryCache(setup =>
            {
                //setup.SizeLimit = 1000;
                //setup.ExpirationScanFrequency.Add()
            });

            // Database
            services.AddDbContext<DbContext, ApplicationContext>(options =>
            {
                options.UseSqlServer(_configuration.GetConnectionString("EmployeeDB"));
            });
            // Identity
            services.AddIdentity<User, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationContext>()
                .AddDefaultTokenProviders();

            services.Configure<IdentityOptions>(opt =>
            {
                // Password
                opt.Password.RequiredLength = 7;
                opt.Password.RequireDigit = false;
                opt.Password.RequireUppercase = false;

                // User
                opt.User.RequireUniqueEmail = true;

                // Sign in
                opt.SignIn.RequireConfirmedEmail = true;

            });

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
                    ValidIssuer = _configuration["JsonWebTokenKeys:ValidIssuer"],

                    ValidateAudience = bool.Parse(_configuration["JsonWebTokenKeys:ValidateAudience"]),
                    ValidAudience = _configuration["JsonWebTokenKeys:ValidAudience"],

                    ValidateLifetime = bool.Parse(_configuration["JsonWebTokenKeys:ValidateLifetime"]),
                    RequireExpirationTime = bool.Parse(_configuration["JsonWebTokenKeys:RequireExpirationTime"]),
                };
            });


            services.Configure<EmployeeController>(config =>
            {
                config.options = new MemoryCacheEntryOptions()
                                    .SetAbsoluteExpiration(TimeSpan.FromSeconds(3600))
                                    .SetSlidingExpiration(TimeSpan.FromSeconds(60))
                                    .SetPriority(CacheItemPriority.Normal)
                                    .SetSize(100);

            });

            //Mail
            var emailConfig = _configuration
                                .GetSection("EmailConfiguration")
                                .Get<EmailConfiguration>();

            services.AddSingleton(emailConfig);
            services.AddScoped<IEmailSender, MailService>();

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