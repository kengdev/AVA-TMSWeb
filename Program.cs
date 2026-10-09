using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Rotativa.AspNetCore;
using TMSWeb.Models.Entities.HR;
using TMSWeb.Models.Entities.LOG;
using TMSWeb.Repositories;
using TMSWeb.Services;

var builder = WebApplication.CreateBuilder(args);

// =====================================================
// MVC
// =====================================================

builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IDispatchService, DemoDispatchService>();
// =====================================================
// Database
// =====================================================

builder.Services.AddDbContext<DbavalogContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("servLOG"));
});

builder.Services.AddDbContext<DbavahrContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("servHR"));
});

// =====================================================
// SSO Configuration
// =====================================================

var ssoConfig =
    builder.Configuration.GetSection("Authentication:Sso");

var authority =
    ssoConfig["Authority"]
    ?? throw new InvalidOperationException(
        "ไม่พบ Authentication:Sso:Authority");

var clientId =
    ssoConfig["ClientId"]
    ?? throw new InvalidOperationException(
        "ไม่พบ Authentication:Sso:ClientId");

var clientSecret =
    ssoConfig["ClientSecret"]
    ?? throw new InvalidOperationException(
        "ไม่พบ Authentication:Sso:ClientSecret");

// =====================================================
// Authentication
// =====================================================

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            CookieAuthenticationDefaults.AuthenticationScheme;

        options.DefaultSignInScheme =
            CookieAuthenticationDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme =
            OpenIdConnectDefaults.AuthenticationScheme;

        options.DefaultSignOutScheme =
            OpenIdConnectDefaults.AuthenticationScheme;
    })

    // =================================================
    // Cookie
    // =================================================

    .AddCookie(
        CookieAuthenticationDefaults.AuthenticationScheme,
        options =>
        {
            options.Cookie.Name =
                "TMSWeb.Auth";

            options.Cookie.HttpOnly =
                true;

            options.Cookie.IsEssential =
                true;

            options.Cookie.SecurePolicy =
                CookieSecurePolicy.Always;

            options.Cookie.SameSite =
                SameSiteMode.Lax;

            options.ExpireTimeSpan =
                TimeSpan.FromHours(8);

            options.SlidingExpiration =
                true;

            options.AccessDeniedPath =
                "/account/access-denied";
        })

    // =================================================
    // OpenID Connect Client
    // =================================================

    .AddOpenIdConnect(
        OpenIdConnectDefaults.AuthenticationScheme,
        options =>
        {
            // =========================================
            // SSO Server
            // =========================================

            options.Authority =
                authority;

            options.ClientId =
                clientId;

            options.ClientSecret =
                clientSecret;

            // =========================================
            // Authorization Code Flow + PKCE
            // =========================================

            options.ResponseType =
                OpenIdConnectResponseType.Code;

            options.UsePkce =
                true;

            // =========================================
            // Sign In ด้วย Cookie ของ TMS
            // =========================================

            options.SignInScheme =
                CookieAuthenticationDefaults
                    .AuthenticationScheme;

            // =========================================
            // Save Tokens
            // =========================================

            options.SaveTokens =
                true;

            // =========================================
            // Require HTTPS metadata
            // =========================================

            options.RequireHttpsMetadata =
                true;

            // =========================================
            // Callback URL
            // =========================================

            options.CallbackPath =
                "/signin-oidc";

            // =========================================
            // Logout callback
            // =========================================

            options.SignedOutCallbackPath =
                "/signout-callback-oidc";

            options.SignedOutRedirectUri =
                "/";

            // =========================================
            // Claims
            // =========================================

            options.MapInboundClaims =
                false;

            options.GetClaimsFromUserInfoEndpoint =
                false;

            // =========================================
            // Scopes
            // =========================================

            options.Scope.Clear();

            options.Scope.Add(
                "openid");

            options.Scope.Add(
                "profile");

            options.Scope.Add(
                "email");

            options.Scope.Add(
                "roles");

            // =========================================
            // Claim Mapping
            // =========================================

            options.TokenValidationParameters =
                new TokenValidationParameters
                {
                    NameClaimType =
                        "name",

                    RoleClaimType =
                        "role"
                };
        });

// =====================================================
// Authorization
// =====================================================

builder.Services.AddAuthorization();

// =====================================================
// Repository / Services
// =====================================================

builder.Services
    .AddApplicationRepositories()
    .AddApplicationServices();

builder.Services.AddScoped<AttendanceExcelService>();

// =====================================================
// Build
// =====================================================

var app = builder.Build();

// =====================================================
// Error Handling
// =====================================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler(
        "/Home/Error");

    app.UseHsts();
}

// =====================================================
// HTTPS
// =====================================================

app.UseHttpsRedirection();

// =====================================================
// Static Files
// =====================================================

app.UseStaticFiles();

// =====================================================
// Routing
// =====================================================

app.UseRouting();

// =====================================================
// Authentication
// ต้องมาก่อน Authorization
// =====================================================

app.UseAuthentication();

// =====================================================
// Authorization
// =====================================================

app.UseAuthorization();

// =====================================================
// MVC Route
// =====================================================

app.MapControllerRoute(
    name: "default",
    pattern:
        "{controller=Home}/{action=Index}/{id?}");

// =====================================================
// Rotativa
// =====================================================

RotativaConfiguration.Setup(
    app.Environment.ContentRootPath,
    "wwwroot/Rotativa");

// =====================================================
// Run
// =====================================================

app.Run();