using Microsoft.EntityFrameworkCore;
using CleanArchitecture.VerticalSlice.Domain.Entities;

namespace CleanArchitecture.VerticalSlice.Infrastructure.Database;

public class ApplicationDbContext(DbContextOptions options)
 : DbContext(options)
{
    public DbSet<Book> Books { get; set; } = null!;
}
