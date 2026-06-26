using Microsoft.Extensions.Hosting;
using Microsoft.Azure.WebJobs;

namespace AlpineSkiHouse.WebJobs
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = new HostBuilder();
            builder.ConfigureWebJobs(b =>
            {
                b.AddAzureStorageCoreServices();
                b.AddAzureStorageQueues();
                b.AddAzureStorageBlobs();
            });
            var host = builder.Build();
            using (host)
            {
                host.Run();
            }
        }
    }
}
