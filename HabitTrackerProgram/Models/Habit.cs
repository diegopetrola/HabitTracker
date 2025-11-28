using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace HabitTrackerProgram.Models;

public class HabitContext : DbContext
{
    public DbSet<Habit> Habit{ get; set; }
    public string DbPath { get; }

    public HabitContext()
    {
        var folder =  Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        DbPath = System.IO.Path.Join(path, "blogging.db");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={DbPath}");
}

public class Habit
{
    public int Id { get; set; }
    public string Description { get; set; } = "";
    public DateOnly Date { get; set; }
    public int Quantity { get; set; }
}
