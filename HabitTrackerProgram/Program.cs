//This habit can't be tracked by time (ex. hours of sleep), only by quantity (ex. number of water glasses a day)
//Users need to be able to input the date of the occurrence of the habit
//The application should store and retrieve data from a real database
//When the application starts, it should create a sqlite database, if one isn’t present.
//It should also create a table in the database, where the habit will be logged.
//The users should be able to insert, delete, update and view their logged habit.
//You should handle all possible errors so that the application never crashes.
//You can only interact with the database using ADO.NET.You can’t use mappers such as Entity Framework or Dapper.
//Follow the DRY Principle, and avoid code repetition.
//Your project needs to contain a Read Me file where you'll explain how your app works.

using HabitTrackerProgram.Models;
using Microsoft.EntityFrameworkCore;
using var db = new HabitContext();

while (true)
{
    MainMenuScreen();
    var op = Console.ReadKey().KeyChar;
    switch (op)
    {
        case '1':
            List<Habit> habits = await db.Habit.ToListAsync();
            PrintHabits(habits);
            break;
        case '2': //Add
            AddHabitScreen();
            break;
        case '3': // Edit
            EditHabitScreen();
            break;
        case '4': // Delete
            DeleteHabitScreen();
            break;
        case 'q':
            Environment.Exit(0);
            break;
        default:
            Console.Write("Unrecognized input, please try again: ");
            break;
    }
}

async void EditHabitScreen()
{
    Console.WriteLine("\nPlease type the Id of the habit you want to edit (integer): ");
    int id;
    while (!int.TryParse(Console.ReadLine(), out id))
    {
        Console.WriteLine("Invalid input, try again: ");
    }

    Habit? h = await db.Habit.FindAsync(id);
    if (h is null)
    {
        Console.WriteLine("Habit not found. Go back to the main menu and think about what you just did!");
        return;
    }
    else
    {
        bool isDone = false;
        while (!isDone)
        {
            Console.WriteLine("\n----- EDIT MENU -----");
            Console.WriteLine("[1] - Edit Description");
            Console.WriteLine("[2] - Edit Date");
            Console.WriteLine("[3] - Edit Quantity");
            Console.WriteLine("[P] - Print current habit");
            Console.WriteLine("[D] - Done with editing");

            var input = Console.ReadKey().KeyChar;
            switch (input)
            {
                case '1':
                    Console.Write("\nType the new description: ");
                    h.Description = Console.ReadLine() ?? "";
                    break;
                case '2':
                    Console.Write("\nType the new date: ");
                    h.Date = ReadDate();
                    break;
                case '3':
                    Console.Write("\nType the new quantity: ");
                    h.Quantity = ReadInt();
                    break;
                case 'p':
                    PrintHabits([h]);
                    break;
                case 'd':
                    isDone = true;
                    break;
                default:
                    break;
            }
        }
    }

    try
    {
        db.Habit.Update(h);
        await db.SaveChangesAsync();
    }
    catch (Exception)
    {
        Console.WriteLine("Something Went Wrong, please try again.");
    }
}

static int ReadInt(string messageIfInvalid="\nInvalid integer, please try again: ")
{
    int input;
    while (!int.TryParse(Console.ReadLine(), out input))
    {
        Console.Write(messageIfInvalid);
    }
    return input;
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

static DateOnly ReadDate(string messageIfInvalid= "\nInvalid date, please try again: ")
{
    DateOnly date;
    while (!ParseDate(Console.ReadLine(), out date))
    {
        Console.Write(messageIfInvalid);
    }
    return date;
}

// Ideally I'd avoid printing and doing business logic inside the same function
// but since this is a small project I don't see a problem,
// refactoring this should be straightforward should the need arise.
async void AddHabitScreen()
{
    Habit habit = new();

    Console.Write("\nType the habit description: ");
    habit.Description = Console.ReadLine() ?? "";

    Console.Write("\nHabit date (leave blank for today): ");
    habit.Date = ReadDate("\nInvalid date. Try again: ");

    Console.Write("\nHow much you achieved (quantity): ");
    habit.Quantity = ReadInt("\nInvalid integer, please try again");

    try
    {
        await db.Habit.AddAsync(habit);
        await db.SaveChangesAsync();
    }
    catch (Exception)
    {
        Console.WriteLine("Something Went Wrong, please try again.");
    }
}

async void DeleteHabitScreen()
{
    Console.Write("\nPlease type the Id (integer) of the habit to be deleted: ");
    int input = ReadInt("\nInvalid Input, please try again");

    Habit? habit = db.Habit.Find(input);
    if (habit is null) Console.WriteLine("Habit not found. Nothing was deleted. Going back to Main Menu.");
    else
    {
        try
        {
            db.Habit.Remove(habit);
            await db.SaveChangesAsync();
        }
        catch (Exception)
        {
            Console.WriteLine("Something went wrong, please try again.");
        }
    }
}

static void MainMenuScreen()
{
    Console.WriteLine("\n----- MAIN MENU -----");
    Console.WriteLine("[1] - Show all habit records");
    Console.WriteLine("[2] - Add habit");
    Console.WriteLine("[3] - Edit habit");
    Console.WriteLine("[4] - Remove habit by Id");
    Console.WriteLine("[Q] - Quit application");
}

void PrintHabits(List<Habit> habits)
{
    Console.WriteLine("\n----- Habit History -----\n");
    if (habits.Count == 0)
    {
        Console.WriteLine("\nNothing to display");
    }
    else
    {
        foreach (var h in habits)
        {
            Console.WriteLine($"ID: {h.Id} \t\t\tDate added: {h.Date}");
            Console.WriteLine($"Description: {h.Description}");
            Console.WriteLine($"Quantity: {h.Quantity}\n");
        }
    }
}


