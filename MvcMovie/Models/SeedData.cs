using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MvcMovie.Data;

namespace MvcMovie.Models;

public static class SeedData
{
    public static void Initialize(IServiceProvider serviceProvider)
    {
        using var context = new MvcMovieContext(
            serviceProvider.GetRequiredService<
                DbContextOptions<MvcMovieContext>>());

        if (context.Movie.Any())
        {
            return;
        }

        context.Movie.AddRange(
            new Movie
            {
                Title = "When Calls the Heart",
                ReleaseDate = DateTime.Parse("2014-1-11"),
                Genre = "Drama",
                Price = 9.99M,
                Rating = "PG"
            },
            new Movie
            {
                Title = "The Pursuit of Happyness",
                ReleaseDate = DateTime.Parse("2006-12-15"),
                Genre = "Drama",
                Price = 12.99M,
                Rating = "PG-13"
            },
            new Movie
            {
                Title = "The Lion King",
                ReleaseDate = DateTime.Parse("1994-6-24"),
                Genre = "Animation",
                Price = 8.99M,
                Rating = "G"
            },
            new Movie
            {
                Title = "Black Panther",
                ReleaseDate = DateTime.Parse("2018-2-16"),
                Genre = "Action",
                Price = 14.99M,
                Rating = "PG-13"
            },
            new Movie
            {
                Title = "The Ten Commandments",
                ReleaseDate = DateTime.Parse("1956-10-5"),
                Genre = "Drama",
                Price = 7.99M,
                Rating = "G"
            }
        );

        context.SaveChanges();
    }
}