using Stripe;
using StripeDemo.Models.PaymentsGateway;
using StripeDemo.Services;

var builder = WebApplication.CreateBuilder(args);

// This is your test secret API key.
StripeConfiguration.ApiKey = "sk_test_51LVreJIU31sah1igfA7aDQMMOnbLklHUwsnBtRXL52ZOYqdKRjfOoOwD0981MD1iPcupjOnG4ik09F6PnGUBhIgw00o14bmzUr";

// Add services to the container.
//builder.Services.AddSingleton<IPaymentsGateway,StripePaymentsGateway>();
builder.Services.AddSingleton<IPaymentsGateway>(provider => new StripePaymentsGateway(
    provider.GetRequiredService<ILogger<StripePaymentsGateway>>(),
    StripeConfiguration.ApiKey));
builder.Services.AddSingleton<IUserInfosService, UserInfosService>();
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// This is your test secret API key.
StripeConfiguration.ApiKey = "sk_test_51LVreJIU31sah1igfA7aDQMMOnbLklHUwsnBtRXL52ZOYqdKRjfOoOwD0981MD1iPcupjOnG4ik09F6PnGUBhIgw00o14bmzUr";


app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
