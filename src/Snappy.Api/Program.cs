using Snappy.Shared.Config;
using Snappy.Shared.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);

builder.Services.Configure<AwsConfig>(builder.Configuration.GetSection("AWS"));

builder.Services.AddTransient<IAlbumService>(_ => new AlbumService(builder.Configuration["AWS:AlbumTableName"]));
builder.Services.AddTransient<IPhotoService>(_ => new PhotoService(builder.Configuration["AWS:PhotoTableName"]));

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(x =>
    {
        x.AddPolicy("default", policy =>
        {
            // policy.WithOrigins(builder.Configuration["CORS:AllowedOrigin"].Split(';'));
            policy.WithOrigins("http://localhost:5173");
            policy.AllowAnyMethod();
            policy.AllowAnyHeader();
            policy.AllowCredentials();
            policy.WithExposedHeaders("x-paging-total", "x-paging-page", "x-paging-pagecount", "x-paging-pagesize");
        });
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.UseCors("default");

app.MapControllers();

app.Run();
