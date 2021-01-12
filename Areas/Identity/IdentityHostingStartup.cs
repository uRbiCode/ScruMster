using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ScruMster.Data;

[assembly: HostingStartup(typeof(ScruMster.Areas.Identity.IdentityHostingStartup))]
namespace ScruMster.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) =>
            {
                services.AddDbContext<ScruMsterContext>(options =>
                    options.UseSqlServer(
                        context.Configuration.GetConnectionString("ScruMsterContextConnection")));

                //services.AddDefaultIdentity<ScruMsterUser>(options => options.SignIn.RequireConfirmedAccount = true)
                //.AddEntityFrameworkStores<ScruMsterContext>();
            });
        }
    }
}