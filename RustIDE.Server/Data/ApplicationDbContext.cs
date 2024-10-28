using Microsoft.EntityFrameworkCore;
using RustIDE.Server.Models.Dto;

namespace RustIDE.Server.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ChatDto> Chats { get; set; }
        public DbSet<MessageDto> Messages { get; set; }
        public DbSet<FileDto> Files { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ChatDto>()
                .HasMany(c => c.Messages)
                .WithOne(m => m.ChatDto)
                .HasForeignKey(m => m.ChatDtoId);
        }
    }
}