//This habit can't be tracked by time (ex. hours of sleep), only by quantity (ex. number of water glasses a day)
//Users need to be able to input the date of the occurrence of the habit
//The application should store and retrieve data from a real database
//When the application starts, it should create a sqlite database, if one isn’t present.
//It should also create a table in the database, where the habit will be logged.
//The users should be able to insert, delete, update and view their logged habit.
//You should handle all possible errors so that the application never crashes.
//You can only interact with the database using ADO.NET.You can’t use mappers such as Entity Framework or Dapper.
//Follow the DRY Principle, and avoid code repetition.
//Your project needs to contain a Read Me file where you'll explain how your app works. Here's a nice example:

using HabitTrackerProgram.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Text.RegularExpressions;
using var db = new HabitContext();

//Habit habit = new();
//habit.Description = "Read";
//habit.Date = DateOnly.FromDateTime(System.DateTime.Now);
//habit.Quantity = 1;

//Console.WriteLine($"My habit: {habit}");

//db.Add(habit);
//await db.SaveChangesAsync();

//habit = await db.Habit.OrderBy(h => h.Id).FirstOrDefaultAsync() ?? throw new Exception("No habit available");

while (true)
{
    PrintMainMenu();
    var op = Console.ReadKey().KeyChar;
    switch (op)
    {
        case '1':
            List<Habit> habits = await db.Habit.ToListAsync();
            PrintHabits(habits);
            break;
        case '2':
            AddHabit();
            break;
        case '3':
            break;
        case '4':
            break;
        case 'q':
            Environment.Exit(0);
            break;
        default:
            Console.WriteLine("Unrecognized input, please try again");
            break;
    }

}

static bool ParseDate(string? strDate, out DateOnly date)
{
    date = DateOnly.FromDateTime(System.DateTime.Now);
    if (strDate is null || strDate == "") return true;
    try
    {
        date = DateOnly.Parse(strDate);
    }
    catch (Exception)
    {
        return false;
    }
    return true;
}

async void AddHabit()
{
    Habit habit = new();

    Console.Write("Type the habit description: ");
    habit.Description = Console.ReadLine() ?? "";

    Console.Write("\nHabit date (leave blank for today): ");
    DateOnly date;
    while (!ParseDate(Console.ReadLine(), out date))
    {
        Console.Write("\nInvalid date. Try again: ");
    }

    habit.Date = date;
    Console.Write("\nHow much you achieved (quantity): ");
    int input;
    while (!int.TryParse(Console.ReadLine(), out input))
    {
        Console.Write("\nInvalid quantity: ");
    }
    habit.Quantity = input;

    await db.Habit.AddAsync(habit);
    await db.SaveChangesAsync();
}

static void PrintMainMenu()
{
    Console.WriteLine("\n----- MAIN MENU -----");
    Console.WriteLine("[1] - Show all habit records");
    Console.WriteLine("[2] - Add habit");
    Console.WriteLine("[3] - Edit habit");
    Console.WriteLine("[4] - Remove habit by ID");
    Console.WriteLine("[Q] - Quit application");
    Console.Write("   ");
}

void PrintHabits(List<Habit> habits)
{
    if (habits.Count == 0)
    {
        Console.WriteLine("Nothing to display");
    }
    else
    {
        foreach (var h in habits)
        {
            Console.Write($"ID: {h.Id} \t\tDate added: {h.Date}");
            Console.Write($"Description: {h.Description}");
            Console.Write($"Quantity: {h.Quantity}");
        }
    }
}


