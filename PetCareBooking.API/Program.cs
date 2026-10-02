
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetCareBooking.API.Middlewares;
using PetCareBooking.Application;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddApplication();


builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder.AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());
});


builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
}).ConfigureApiBehaviorOptions(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        // Thu thập tất cả thông báo lỗi từ ModelState
        var errors = context.ModelState
            .Where(e => e.Value != null && e.Value.Errors.Count > 0)
            .SelectMany(x => x.Value!.Errors)
            .Select(x => x.ErrorMessage)
            .ToList();

        // Đóng gói vào format chuẩn của hệ thống
        var apiResponse = new ApiResponse<object>
        {
            IsSuccess = false,
            StatusCode = 400,
            // Nối các lỗi lại thành 1 chuỗi dễ đọc, hoặc bạn có thể tạo thêm property List<string> Errors trong ApiResponse
            Message = string.Join(" | ", errors),
            Result = null
        };

        return new BadRequestObjectResult(apiResponse);
    };
});





var app = builder.Build();

app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthorization();


app.MapControllers();

app.Run();
