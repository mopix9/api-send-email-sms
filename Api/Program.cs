using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NotificationService.Services; // برای سرویس‌های SmsService و EmailService
using Api.Models; // برای مدل‌های SmsMessage و EmailMessage
using Microsoft.OpenApi.Models;
using NotificationService.Filters;

var builder = WebApplication.CreateBuilder(args);

// اضافه کردن سرویس‌های ضروری
builder.Services.AddControllers();

// اضافه کردن Swagger برای مستندسازی API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Notification API", Version = "v1" });
});

// اضافه کردن سرویس‌های SmsService و EmailService
builder.Services.AddSingleton<SmsService>();
builder.Services.AddSingleton<EmailService>();
// افزودن فیلتر خطای سفارشی
builder.Services.AddControllers(options =>
{
    options.Filters.Add<CustomExceptionFilter>();
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


