using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace DevTaskManager.Api.Tests;

// Factory de integração: sobe a API real em memória com um banco SQLite isolado por execução
public class ApiWebApplicationFactory : WebApplicationFactory<Program>
{
    public string DbPath { get; }

    public ApiWebApplicationFactory()
    {
        DbPath = Path.Combine(Path.GetTempPath(), $"devtask-tests-{Guid.NewGuid()}.db");
    }

    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = $"Data Source={DbPath}"
            });
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (File.Exists(DbPath)) File.Delete(DbPath);
    }
}
