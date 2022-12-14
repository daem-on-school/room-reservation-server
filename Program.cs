using Microsoft.AspNetCore.Identity;
using RoomReservation;
using RoomReservation.Services;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services
    .AddIdentity<IdentityUser, IdentityRole>(options => {
        options.SignIn.RequireConfirmedAccount = false;
        options.User.RequireUniqueEmail = true;
        options.Password.RequireDigit = false;
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
    })
    .AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddScoped<ReservationService, ReservationService>();

builder.Services
    .ConfigureApplicationCookie(options => {
        // Cookie settings
        options.Cookie.HttpOnly = true;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);//TimeSpan.FromMinutes(5);

        options.LoginPath = "/Identity/Account/Login";
        options.AccessDeniedPath = "/Identity/Account/AccessDenied";
        options.SlidingExpiration = true;
        options.Events.OnRedirectToLogin = context => {
            context.Response.StatusCode = 401;
            return Task.CompletedTask;
        };
    });

builder.Services.AddCors(options => {
    options.AddPolicy("MyPolicy", builder => {
        builder.SetIsOriginAllowed(origin => true)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .WithExposedHeaders("Location")
            .AllowCredentials();
    });
});
builder.Services.AddControllers();
builder.Services.AddDbContextFactory<AppDbContext>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => {
    options.SupportNonNullableReferenceTypes();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors("MyPolicy");
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
