﻿using KledijModule.Common.Authorization;
using KledijModule.Common.RequestContext;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using ProjectCore;
using ProjectCore.Data;
using ProjectCore.Domain.Entities.Gebruiker;
using ProjectCore.Shared.RequestContext;
using System.Security.Claims;

namespace KledijModule
{
    public static class StartupExtensions
    {
        public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
        {
            //to call graph api
            var initialScopes = builder.Configuration["DownstreamApi:Scopes"]?.Split(' ') ?? builder.Configuration["MicrosoftGraph:Scopes"]?.Split(' ');

            // Add services to the container.
            builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"))
                    .EnableTokenAcquisitionToCallDownstreamApi(initialScopes)
                        .AddMicrosoftGraph(builder.Configuration.GetSection("DownstreamApis:MicrosoftGraph"))
                        .AddInMemoryTokenCaches();



            builder.Services.Configure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options =>
            {
                options.Events = new OpenIdConnectEvents
                {
                    OnTokenValidated = async ctx =>
                    {
                        Console.Write("after tokenvalidated");
                        var email = ctx.Principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
                        var userId = ctx.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                        var givenName = ctx.Principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value;
                        var surname = ctx.Principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Surname)?.Value;

                        if (email != null && userId != null)
                        {
                            var db = ctx.HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();
                            var user = await db.Gebruikers.SingleOrDefaultAsync(u => u.Id == userId);

                            if (user == null)
                            {
                                user = new ApplicationUser
                                {
                                    Id = userId,
                                    Email = email,
                                    VoorNaam = givenName ?? string.Empty,
                                    AchterNaam = surname ?? string.Empty,
                                    Balans = 0 // Default balance
                                };
                                db.Gebruikers.Add(user);
                                await db.SaveChangesAsync();
                            }
                        }
                    }
                };
            });


            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy(AuthorizationPolicies.RequireAdminRole, policy => policy.RequireRole(Roles.Admin));
                //options.AddPolicy(AuthorizationPolicies.RequireUserRole, policy => policy.RequireRole(Roles.User)); --> Might not be necessary as I will set user access based on group. every user will be user.

                // By default, all incoming requests will be authorized according to the default policy.
                options.FallbackPolicy = options.DefaultPolicy;
            });
            builder.Services.AddRazorPages(options =>
            {
                options.Conventions.AuthorizeAreaFolder("Admin","/", AuthorizationPolicies.RequireAdminRole);
            }).AddMicrosoftIdentityUI();

            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<IRequestContext, RequestContextService>();
            builder.Services.AddApplicationServices(builder.Configuration);
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict;
                options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;
            });

            //var PhysicalFilesProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), builder.Configuration.GetValue<string>("StoredFilesPath")));
            //builder.Services.AddSingleton<IFileProvider>(PhysicalFilesProvider);
            return builder.Build();
        }

        public static WebApplication ConfigurePipeline(this WebApplication app)
        {

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
                using (var scope = app.Services.CreateScope())
                {
                    var services = scope.ServiceProvider;
                    try
                    {
                        var context = services.GetRequiredService<ApplicationDbContext>();
                        if (!context.Database.CanConnect())
                        {
                            // This will ensure the database is created and all migrations are applied
                            context.Database.Migrate();
                        }
                    }
                    catch (Exception ex)
                    {
                        var logger = services.GetRequiredService<ILogger<Program>>();
                        logger.LogError(ex, "An error occurred while applying migrations.");
                    }
                }
           

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.MapRazorPages();
            app.MapControllers();

            return app;
        }
    }
}
