﻿using AuthApi.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresConnection")));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<EmailService>();
builder.Services.AddSignalR();

var app = builder.Build();

app.Urls.Add("http://0.0.0.0:5000");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting(); // Важно добавить вызов UseRouting

app.UseAuthorization();

app.MapControllers();

app.MapHub<ChatHub>("/chatHub");

app.Run();
