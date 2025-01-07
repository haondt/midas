using Haondt.Identity.StorageKey;
using Haondt.Web.Core.Middleware;
using Haondt.Web.Extensions;
using SpendLess.Domain.Shared.Extensions;
using SpendLess.Extensions;
using SpendLess.Persistence.Extensions;
using SpendLess.UI.Extensions;
using SpendLess.UI.Shared.Extensions;

const string CORS_POLICY = "_spendLessPolicy";

StorageKeyConvert.DefaultSerializerSettings = new StorageKeySerializerSettings
{
    TypeNameStrategy = TypeNameStrategy.SimpleTypeConverter,
    KeyEncodingStrategy = KeyEncodingStrategy.String
};

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers()
    .AddApplicationPart(typeof(Haondt.Web.Extensions.ServiceCollectionExtensions).Assembly)
    .AddApplicationPart(typeof(Haondt.Web.BulmaCSS.Extensions.ServiceCollectionExtensions).Assembly)
    .AddApplicationPart(typeof(SpendLess.Api.Controllers.SpendLessApiController).Assembly);

builder.Configuration.AddEnvironmentVariables();

builder.Services
    .AddHaondtWebServices(builder.Configuration)
    .AddSpendLessServices(builder.Configuration)
    .AddSpendLessDomainServices(builder.Configuration)
    .AddSpendLessPersistenceServices(builder.Configuration)
    .AddSpendLessUI(builder.Configuration)
    .AddSpendLessSharedUI(builder.Configuration);

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
