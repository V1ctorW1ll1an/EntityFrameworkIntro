using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

// Create the factory
var factory = new CookBookContextFactory();

// Create the context
using var context = factory.CreateDbContext(args);

Console.WriteLine("Add porridge for breakfast");

var porridge = new Dish
{
    Title = "Breakfast porridge",
    Notes = "This is so good",
    Stars = 4
};

// Add
context.Dishes.Add(porridge);
await context.SaveChangesAsync();
Console.WriteLine($"Add porridge successfully: {porridge.Id}");

// Read
var dishes = await context.Dishes.Where(d => d.Title.Contains("porridge")).ToListAsync();
if (dishes.Count != 1)
    Console.Error.WriteLine("Something really bad happened. Porridge disappeared :-(");
dishes.ForEach(item => Console.WriteLine($"{item.Title}"));

// Update
Console.WriteLine($"number of star before update {porridge.Stars}");
porridge.Stars = 5;
await context.SaveChangesAsync();
Console.WriteLine($"number of star: {porridge.Stars}");

Console.WriteLine("Removing porridge from database");
context.Dishes.Remove(porridge);
Console.WriteLine("porridge removed");

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
    public CookBookContext CreateDbContext(string[] args)
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
