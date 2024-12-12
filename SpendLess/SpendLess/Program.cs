using Haondt.Identity.StorageKey;
using Haondt.Web.Core.Middleware;
using Haondt.Web.Extensions;
using SpendLess.Accounts.Extensions;
using SpendLess.Admin.Extensions;
using SpendLess.Dashboard.Extensions;
using SpendLess.Domain.Extensions;
using SpendLess.Extensions;
using SpendLess.Kvs.Extensions;
using SpendLess.NodeRed.Extensions;
using SpendLess.Persistence.Extensions;
using SpendLess.TransactionImport.Extensions;
using SpendLess.Transactions.Extensions;
using SpendLess.Web.Domain.Extensions;

const string CORS_POLICY = "_fireflyIIIPPPolicy";

StorageKeyConvert.DefaultSerializerSettings = new StorageKeySerializerSettings
{
    TypeNameStrategy = TypeNameStrategy.SimpleTypeConverter,
    KeyEncodingStrategy = KeyEncodingStrategy.String
};

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers()
    .AddApplicationPart(typeof(Haondt.Web.Extensions.ServiceCollectionExtensions).Assembly)
    .AddApplicationPart(typeof(Haondt.Web.BulmaCSS.Extensions.ServiceCollectionExtensions).Assembly)
    .AddApplicationPart(typeof(SpendLess.Web.Domain.Extensions.ServiceCollectionExtensions).Assembly);
//.AddNewtonsoftJson(options =>
//{
//    options.SerializerSettings.ConfigureFireflyppRunnerSettings();
//});

builder.Configuration.AddEnvironmentVariables();

builder.Services
    .AddHaondtWebServices(builder.Configuration)
    .AddSpendLessServices(builder.Configuration)
    .AddSpendLessPersistenceServices(builder.Configuration)
    .AddSpendLessDomainServices()
    .AddAccounts(builder.Configuration)
    .AddKvs(builder.Configuration)
    .AddAdmin(builder.Configuration)
    .AddDashboard(builder.Configuration)
    .AddSpendLessWebDomainServices(builder.Configuration)
    .AddNodeRedServices(builder.Configuration)
    .AddTransactionImport()
    .AddTransactions(builder.Configuration);
//.AddNodeRedServices(builder.Configuration)
//.AddLookupServices(builder.Configuration);
//.AddFireflyIIIPPWebServices(builder.Configuration)
//.AddCoreServices(builder.Configuration)
//.AddFireflyIIIServices(builder.Configuration)
//.AddNodeRedServices(builder.Configuration)
//.AddFireflyIIIPPRunnerServices(builder.Configuration)
//.AddFilePersistenceServices();


builder.Services.AddMvc();
builder.Services.AddCors(o => o.AddPolicy(CORS_POLICY, p =>
{
    p.AllowAnyOrigin();
    p.AllowAnyHeader();
}));
builder.Services.AddServerSideBlazor();


var app = builder.Build();

app.UseStaticFiles();
app.UseCors(CORS_POLICY);
app.MapControllers();
app.UseMiddleware<ExceptionHandlerMiddleware>();


app.Run();
