using CSharpCodeChallenge.Api.Services;
using CSharpCodeChallenge.Api.Validation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IPublishRequestValidator, PublishRequestValidator>();
builder.Services.AddSingleton<IPublishedItemXmlGenerator, PublishedItemXmlGenerator>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();

public partial class Program;
