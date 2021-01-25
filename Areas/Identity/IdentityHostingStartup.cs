using Microsoft.AspNetCore.Hosting;

[assembly: HostingStartup(typeof(ScruMster.Areas.Identity.IdentityHostingStartup))]
namespace ScruMster.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) =>
            {
                //services.AddDbContext<ScruMsterContext>(options =>
                //options.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=ScruMster;Trusted_Connection=True;MultipleActiveResultSets=true"));

                //services.AddDefaultIdentity<ScruMsterUser>(options => options.SignIn.RequireConfirmedAccount = true)
                //.AddEntityFrameworkStores<ScruMsterContext>();
            });
        }
    }
}