using Microsoft.EntityFrameworkCore;

namespace HabitTrackerProgram.Models;

public class HabitContext : DbContext
{
    public DbSet<Habit> Habit { get; set; }
    public DbSet<HabitType> HabitType { get; set; }
    public string DbPath { get; }

    public HabitContext()
    {
        var path = Environment.CurrentDirectory;
        DbPath = System.IO.Path.Join(path, "habit.db");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={DbPath}");
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        List<HabitType> defaultTypes = new List<HabitType> {
            new HabitType { Id = 1, Name = "Running", UnityName = "Km" },
            new HabitType { Id = 2, Name = "Eating", UnityName = "KCal" },
            new HabitType { Id = 3, Name = "Reading", UnityName = "Books" },
            new HabitType { Id = 4, Name = "Exercising", UnityName = "Minutes" }
        };

        modelBuilder.Entity<HabitType>().HasData(defaultTypes);
    }
}

public class Habit
{
    public int Id { get; set; }
    public DateOnly Date { get; set; }
    public int Quantity { get; set; }
    public int HabitTypeId { get; set; }
    public HabitType HabitType { get; set; }

}

public class HabitType
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string UnityName { get; set; } = "";
}

