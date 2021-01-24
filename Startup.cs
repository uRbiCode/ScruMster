using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ScruMster.Areas.Identity.Data;
using ScruMster.Data;
using System;
using System.Threading.Tasks;

namespace ScruMster
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

            services.AddControllersWithViews(); //zamiana
            services.AddRazorPages();

            //2Fa
            //services.AddTransient<IEmailSender, YourEmailSender>();
            //services.AddTransient<IEmailSender, YourSmsSender>();
            //services.AddScoped<IUserClaimsPrincipalFactory<ScruMsterUser>>();

            services.AddDbContext<ScruMsterContext>(options =>
            options.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=ScruMster;Trusted_Connection=True;MultipleActiveResultSets=true"));



            services.AddIdentity<ScruMsterUser, IdentityRole>()
                 .AddDefaultUI()
                 .AddEntityFrameworkStores<ScruMsterContext>()
                 .AddDefaultTokenProviders();

            services.Configure<IdentityOptions>(options =>
            {
                // Password settings.
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 1;

                // Lockout settings.
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = false;

                // User settings.
                options.User.AllowedUserNameCharacters =
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = false;
            });

            services.ConfigureApplicationCookie(options =>
            {
                // Cookie settings
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(5);

                options.LoginPath = "/Identity/Account/Login";
                options.AccessDeniedPath = "/Identity/Account/AccessDenied";
                options.SlidingExpiration = true;
            });
        }

        private async Task CreateUserRoles(IServiceProvider serviceProvider)
        {
            // Initializing custom roles   
            var RoleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            IdentityResult roleResultAdmin;
            IdentityResult roleResultManager;
            IdentityResult roleResultUser;

            // Adding Admin Role
            var roleCheckAdmin = await RoleManager.RoleExistsAsync("Admin");
            var roleCheckManager = await RoleManager.RoleExistsAsync("Manager");
            var roleCheckUser = await RoleManager.RoleExistsAsync("User");
            if (!roleCheckAdmin)
            {
                //Create the roles and seed them to the database 
                roleResultAdmin = await RoleManager.CreateAsync(new IdentityRole("Admin"));
            }
            if (!roleCheckManager)
            {
                //Create the roles and seed them to the database 
                roleResultManager = await RoleManager.CreateAsync(new IdentityRole("Manager"));
            }
            if (!roleCheckUser)
            {
                //Create the roles and seed them to the database 
                roleResultUser = await RoleManager.CreateAsync(new IdentityRole("User"));
            }
        }

        private async Task CreateAdminAccount(IServiceProvider serviceProvider)
        {
            var UserManager = serviceProvider.GetRequiredService<UserManager<ScruMsterUser>>();
            //Create and add admin user

            var adminUser = await UserManager.FindByIdAsync("AdminID");
            await UserManager.AddToRoleAsync(adminUser, "Admin");
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider services)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseStatusCodePagesWithReExecute("/Error/{0}");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            CreateUserRoles(services).Wait();
            CreateAdminAccount(services).Wait();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }
    }
}
