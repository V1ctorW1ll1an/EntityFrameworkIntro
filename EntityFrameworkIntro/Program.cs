using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

// Create the factory
var factory = new CookBookContextFactory();

// Create the context
using var context = factory.CreateDbContext(args);

var newDish = new Dish { Title = "Foo", Notes = "Bar" };
context.Dishes.Add(newDish);
await context.SaveChangesAsync();

newDish.Notes = "Baz";
await context.SaveChangesAsync();

await EntityStates(factory);
await ChangeTracking(factory);

static async Task EntityStates(CookBookContextFactory dbFactory)
{
    using var dbContext = dbFactory.CreateDbContext();
    var newDish = new Dish { Title = "Foo", Notes = "Bar" };

    var state = dbContext.Entry(newDish).State; // detached

    dbContext.Dishes.Add(newDish);
    state = dbContext.Entry(newDish).State; // added

    await dbContext.SaveChangesAsync();
    state = dbContext.Entry(newDish).State; // unchanged

    newDish.Notes = "Other Bar";
    state = dbContext.Entry(newDish).State; // modified

    await dbContext.SaveChangesAsync();
    state = dbContext.Entry(newDish).State; // unchanged

    dbContext.Remove(newDish);
    state = dbContext.Entry(newDish).State; // deleted

    await dbContext.SaveChangesAsync();
    state = dbContext.Entry(newDish).State; // detached
}

static async Task ChangeTracking(CookBookContextFactory dbFactory)
{
    using var dbContext = dbFactory.CreateDbContext();

    var newDish = new Dish { Title = "Foo", Notes = "Bar" };
    dbContext.Dishes.Add(newDish);
    await dbContext.SaveChangesAsync();

    newDish.Notes = "Bar";
    var entry = dbContext.Entry(newDish);
    var originalValue = entry.OriginalValues[nameof(Dish.Notes)].ToString();
}

#region Models
class Dish
{
    public int Id { get; set; }

    [MaxLength(100)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Notes { get; set; }

    public int? Stars { get; set; }

    public List<DishIngredients>? Ingredients { get; set; } = new();
}

class DishIngredients
{
    public int Id { get; set; }

    [MaxLength(100)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(50)]
    public string UniOfMeasure { get; set; } = string.Empty;

    [Column(TypeName = "decimal(5,2)")]
    public decimal Amount { get; set; }

    public Dish? Dish { get; set; }

    public int DishId { get; set; }
}

#endregion

#region Context
class CookBookContext : DbContext
{
    public DbSet<Dish> Dishes { get; set; }

    public DbSet<DishIngredients> Ingredients { get; set; }

    public CookBookContext(DbContextOptions<CookBookContext> options)
        : base(options) { }
}

#endregion

#region Factory
class CookBookContextFactory : IDesignTimeDbContextFactory<CookBookContext>
{
    public CookBookContext CreateDbContext(string[]? args = null)
    {
        var builder = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", false, true)
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<CookBookContext>();
        // postgresql
        optionsBuilder.UseNpgsql(builder.GetConnectionString("DefaultConnection"));

        return new CookBookContext(optionsBuilder.Options);
    }
}
#endregion
