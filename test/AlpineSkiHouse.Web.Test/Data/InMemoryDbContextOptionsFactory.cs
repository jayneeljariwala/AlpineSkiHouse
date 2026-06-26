using Microsoft.EntityFrameworkCore;

namespace AlpineSkiHouse.Web.Tests.Data
{
    public static class InMemoryDbContextOptionsFactory
    {
        public static DbContextOptions<T> Create<T>() where T : DbContext
        {
            var builder = new DbContextOptionsBuilder<T>();
            builder.UseInMemoryDatabase(System.Guid.NewGuid().ToString());
            return builder.Options;
        }
    }
}
