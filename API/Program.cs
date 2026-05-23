using DataAccess;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
Console.WriteLine(builder.Configuration);
builder.Services.AddDataAccess(builder.Configuration);
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "ConditerTrans API v1");
    options.RoutePrefix = "swagger";
});

app.MapControllers();
app.Run();
