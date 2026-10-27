Console.WriteLine("Hello, World!");
Console.WriteLine("The current time is " + DateTime.Now);
Console.WriteLine($"There are {(new DateTime(DateTime.Now.Year, 12, 25) - DateTime.Now).Days} days until Christmas.");

Console.Write("What is your favorite color? ");
string? favoriteColor = Console.ReadLine();
Console.WriteLine("Oh, I love " + favoriteColor + " too!");

Console.Write("What is your name? ");
string? name = Console.ReadLine();
Console.WriteLine($"Hello, {name}! Nice to meet you.");