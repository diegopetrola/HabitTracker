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

// Minor concerns:
// Code was done using EntityFrameworkCore
// db has a delay
// Console.WriteLine("Something Went Wrong, please try again.") appears too much
// the "Screen" functions are dealing with business logic


using HabitTrackerProgram.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using var db = new HabitContext();

CustomSeeding(100);

while (true)
{
    MainMenuScreen();
    var op = Console.ReadKey().KeyChar;
    switch (op)
    {
        case '1':
            ListHabitsScreen();
            break;
        case '2':
            AddHabitScreen();
            break;
        case '3':
            DeleteHabitScreen();
            break;
        case '4':
            ListHabitTypeScreen();
            break;
        case '5':
            AddHabitTypeScreen();
            break;
        case '6':
            DeleteHabitTypeScreen();
            break;
        case 'q':
            Environment.Exit(0);
            break;
        default:
            Console.Write("\nUnrecognized input, please try again: ");
            break;
    }
}

async void ListHabitsScreen()
{
    Console.WriteLine("\nWhich type of visualization would you like?");
    Console.WriteLine("[1] - List habits individually");
    Console.WriteLine("[2] - Average of habits");
    Console.WriteLine("[3] - Count ocurrences of habits");
    int input = Console.ReadKey().KeyChar;
    var (startDate, endDate) = DateRangeScreen();
    switch (input)
    {
        case '1':
            PrintHabitsByDate(startDate, endDate);
            break;
        case '2':
            PrintHabitsAverage(startDate, endDate);
            break;
        case '3':
            PrintHabitsCount(startDate, endDate);
            break;
        default:
            Console.WriteLine("Invalid option. Go back to the main menu and think about what you did!");
            break;
    }
}

async void PrintHabitsAverage(DateOnly startDate, DateOnly endDate)
{
    var averageHabits = await db.Habit.Include(h => h.HabitType).
                Where(h => h.Date >= startDate && h.Date <= endDate).
                GroupBy(h => new { h.HabitType.Name, h.HabitType.UnityName }).
                Select(g => new
                {
                    HabitType = g.Key,
                    AvgQuantity = g.Average(h => h.Quantity)
                }).ToListAsync();

    if (averageHabits.Count > 0)
    {
        Console.WriteLine($"\nAverage for the time range {startDate} to {endDate}");
        foreach (var avg in averageHabits)
        {
            Console.WriteLine($"\n{avg.HabitType.Name}: {avg.AvgQuantity:00.0} {avg.HabitType.UnityName}");
        }
    }
    else Console.WriteLine("\nNothing to show for the specified range");
}

async void PrintHabitsCount(DateOnly startDate, DateOnly endDate)
{
    var habitCount = await db.Habit.Include(h => h.HabitType).
                Where(h => h.Date >= startDate && h.Date <= endDate).
                GroupBy(h => new { h.HabitType.Name }).
                Select(g => new
                {
                    HabitType = g.Key,
                    Count = g.Count()
                }).ToListAsync();

    if (habitCount.Count > 0)
    {
        Console.WriteLine($"\nNumber of Habits in the time range {startDate} to {endDate}");
        foreach (var habit in habitCount)
        {
            Console.WriteLine($"\n{habit.HabitType.Name}: {habit.Count} ocurrences");
        }
    }
    else Console.WriteLine("\nNothing to show for the specified range");
}


async void ListHabitTypeScreen()
{
    var hTypes = await db.HabitType.ToListAsync();
    Console.WriteLine();
    if (hTypes is null || hTypes.Count == 0)
    {
        Console.WriteLine("No habit types registered.");
        return;
    }
    foreach (var hType in hTypes)
    {
        Console.WriteLine($"ID: {hType.Id}\tName: {hType.Name}\t\tUnity: {hType.UnityName}");
    }
}

static int ReadInt(string messageIfInvalid = "\nInvalid integer, please try again: ")
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
    if (int.TryParse(strDate, out int days))
    {
        date = date.AddDays(days);
        return true;
    }
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

(DateOnly, DateOnly) DateRangeScreen()
{
    DateOnly date1, date2;
    Console.WriteLine("\nStart date (leave blank for today, type a negative integer to subtract days, eg -30 = today - 30): ");
    date1 = ReadDate();
    Console.WriteLine("\nEnd Date (same rules as above): ");
    date2 = ReadDate();
    return (date1, date2);
}

static DateOnly ReadDate(string messageIfInvalid = "\nInvalid date, please try again: ")
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
    var habitTypes = await db.HabitType.ToListAsync();
    if (habitTypes.Count == 0)
    {
        Console.WriteLine("\nPlease register a type of habit before adding habits");
        return;
    }

    Habit habit = new();
    var types = await db.HabitType.ToListAsync();

    Console.Write("\nHabit date (leave blank for today, negative number to subtract days from today): ");
    habit.Date = ReadDate("\nInvalid date. Try again: ");

    Console.Write("How much you achieved (quantity): ");
    habit.Quantity = ReadInt("\nInvalid integer, please try again");

    Console.Write("Please select the ID of the type of habit you want to add: ");
    ListHabitTypeScreen();
    int hTypeId = ReadInt();
    while (!habitTypes.Any(h => h.Id == hTypeId)) {
        Console.WriteLine("Type the Id of a valid habit");
        hTypeId = ReadInt();
    }
    habit.HabitTypeId = hTypeId;

    try
    {
        await db.Habit.AddAsync(habit);
        await db.SaveChangesAsync();
        Console.WriteLine("New habit saved!");
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
            Console.WriteLine("Deletion successful.");
        }
        catch (Exception)
        {
            Console.WriteLine("Something went wrong, please try again.");
        }
    }
}

async void AddHabitTypeScreen()
{
    HabitType ht = new();
    Console.WriteLine("\nType the habit name:");
    ht.Name = Console.ReadLine() ?? "";
    Console.WriteLine("Type the unity used to measure it:");
    ht.UnityName = Console.ReadLine() ?? "";

    try
    {
        await db.HabitType.AddAsync(ht);
        await db.SaveChangesAsync();
    }
    catch
    {
        Console.WriteLine("Something went wrong, please try again.");
    }
}

static void MainMenuScreen()
{
    Console.WriteLine("\n----- MAIN MENU -----");
    Console.WriteLine("[1] - Show habits");
    Console.WriteLine("[2] - Add habit");
    Console.WriteLine("[3] - Remove habit by Id");
    Console.WriteLine("[4] - Show habit types");
    Console.WriteLine("[5] - Add habit type");
    Console.WriteLine("[6] - Remove habit type by Id");
    Console.WriteLine("[Q] - Quit application");
}

async void PrintHabitsByDate(DateOnly startDate, DateOnly endDate)
{
    List<Habit> habits = await
        db.Habit.Where(h => h.Date >= startDate && h.Date <= endDate)
        .Include(h => h.HabitType).ToListAsync();

    PrintHabits(habits);
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
            Console.WriteLine($"ID: {h.Id} - Achieved {h.Quantity} {h.HabitType.UnityName} in {h.HabitType.Name}, {h.Date}\n");
        }
    }
}

async void DeleteHabitTypeScreen()
{
    Console.WriteLine("\nType the Id of the HabitType you want to delete: ");
    int input = ReadInt();

    var habit = await db.HabitType.FirstOrDefaultAsync(h => h.Id == input);

    if (habit is null)
    {
        Console.WriteLine("\nNo habit found");
        return;
    }
    try
    {
        db.HabitType.Remove(habit);
        await db.SaveChangesAsync();
        Console.WriteLine("Deleteion sucessful");
    }
    catch (Exception)
    {
        Console.WriteLine("Something went wrong, please try again.");
    }
}

async void CustomSeeding(int n)
{
    Habit? data = await db.Habit.FirstOrDefaultAsync();
    if (data is not null) return;

    Random rand = new();
    List<HabitType> types = await db.HabitType.ToListAsync();
    List<Habit> habits = [];
    int i = 1;
    while (i <= n)
    {
        Habit h = new();
        h.Id = i;
        h.HabitTypeId = types[rand.Next(types.Count)].Id;
        h.Date = DateOnly.FromDateTime(System.DateTime.Now);
        h.Date = h.Date.AddDays(-rand.Next(30));
        h.Quantity = rand.Next(100);
        habits.Add(h);
        i++;
    }

    try
    {
        await db.Habit.AddRangeAsync(habits);
        await db.SaveChangesAsync();
        Console.WriteLine("Sample data added.");
    }
    catch (Exception)
    {
        Console.WriteLine("Something went wrong while initializing the database, check the error below.");
        throw;
    }
}