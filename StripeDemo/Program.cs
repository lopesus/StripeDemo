using Stripe;
using StripeDemo.Models.PaymentsGateway;
using StripeDemo.Models.ViewModels;
using StripeDemo.Services;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
//builder.Services.AddSingleton<IPaymentsGateway,StripePaymentsGateway>();
builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection(StripeSettings.SettingsOptions));
var stripeSettings = builder.Configuration.GetSection(StripeSettings.SettingsOptions).Get<StripeSettings>();

//set stripe api key
StripeConfiguration.ApiKey = stripeSettings.ApiKey;


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


app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
