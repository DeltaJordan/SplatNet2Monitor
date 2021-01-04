using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Annaki.Web
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            await Annaki.Start();

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    X509Certificate2 cert = X509Certificate2.CreateFromPemFile(
                        "/etc/ssl/certs/kelpdo.me.pem",
                        "/etc/ssl/certs/kelpdo.me.key");

                    webBuilder.UseKestrel(options =>
                    {
                        options.ConfigureHttpsDefaults(httpsOptions => { httpsOptions.ServerCertificate = cert; });
                        options.ConfigureEndpointDefaults(listenOptions => { listenOptions.UseHttps(cert); });
                    });
                    webBuilder.UseStartup<Startup>();
                });
    }
}
