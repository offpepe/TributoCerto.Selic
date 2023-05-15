using TributoCerto.Selic.Data;
using TributoCerto.Selic.Data.Interfaces;
using TributoCerto.Selic.Jobs;
using TributoCerto.Selic.Service;
using TributoCerto.Selic.Service.Interfaces;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddTransient<ISelicRepository, SelicRepository>();
builder.Services.AddTransient<ISelicApi, SelicApi>();
builder.Services.AddTransient<ISelicService, SelicService>();
builder.Services.AddCronJob<UpdateSelicTableJob>(c =>
{
    c.CronExpression = "0 0 1 * *";
    c.TimeZoneInfo = TimeZoneInfo.Local;
});
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();