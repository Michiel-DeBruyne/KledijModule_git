using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ProjectCore.Domain.Common;
using ProjectCore.Domain.Entities.Bestellingen;
using ProjectCore.Domain.Entities.Catalogus;
using ProjectCore.Domain.Entities.Gebruiker;

//using ProjectCore.Domain.Entities.Gebruiker;
using ProjectCore.Domain.Entities.WebShop;
using ProjectCore.Domain.Entities.WinkelMand;
using ProjectCore.Shared.RequestContext;

namespace ProjectCore.Data;

public class ApplicationDbContext : DbContext
{
    private readonly IRequestContext _requestContext;
    private readonly IConfiguration _configuration;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IRequestContext requestContext, IConfiguration configuration)
        : base(options)
    {
        _requestContext = requestContext;
        _configuration = configuration;
        // Dit niet verwijderen pls. anders gaan navigation properties hun data laden en gecombineerd met de shoppingcartsummarylist, krijg je een stack overflow omdat je een enorme loop krijgt.
        this.ChangeTracker.LazyLoadingEnabled = false;
    }

    public DbSet<WebShopConfiguration> WebShopConfigurations { get; set; }
    public DbSet<Categorie> Categories { get; set; }
    public DbSet<Product> Producten { get; set; }
    public DbSet<Foto> ProductFotos { get; set; }
    public DbSet<ProductKleur> ProductKleuren { get; set; }
    public DbSet<ProductMaat> ProductMaten { get; set; }

    public DbSet<ShoppingCart> ShoppingCarts { get; set; }
    public DbSet<ShoppingCartItem> ShoppingCartItems { get; set; }

    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }

    public DbSet<ApplicationUser> Gebruikers { get; set; }

    public DbSet<Favoriet> Favorieten { get; set; }


    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(builder);

        // Customize the ASP.NET Identity model and override the defaults if needed.
        // For example, you can rename the ASP.NET Identity table names and more.
        // Add your customizations after calling base.OnModelCreating(builder);
    }

    //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //{
    //    // Haal tenant op en gebruik de bijbehorende connectiestring
    //    var tenant = _requestContext.Zone;
    //    if (string.IsNullOrEmpty(tenant))
    //    {
    //        throw new InvalidOperationException("Zone not found");
    //    }

    //    var connectionString = _configuration.GetConnectionString(tenant);
    //    if (string.IsNullOrEmpty(connectionString))
    //    {
    //        throw new InvalidOperationException($"Connection string for tenant '{tenant}' not found.");
    //    }

    //    optionsBuilder.UseSqlServer(connectionString);
    //    base.OnConfiguring(optionsBuilder);
    //}

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        string userName = _requestContext.UserName;
        TimeZoneInfo cest = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedBy = userName;
                    entry.Entity.CreatedDate = DateTime.Now; /*TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, cest);*/
                    break;
                case EntityState.Modified:
                    entry.Entity.LastModifiedBy = userName;
                    entry.Entity.LastModifiedDate = DateTime.Now;/*TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, cest);*/
                    break;
            }
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}
