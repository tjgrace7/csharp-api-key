using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

public class AppDbContext : DbContext
{

    public AppDbContext(DbContextOptions<AppDbContext> options) : base (options) { }

    public DbSet<Person> People => Set<Person>();
    public DbSet<Ticket> Tickets => Set<Ticket>();

    public DbSet<TicketUpdate> Updates => Set<TicketUpdate>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Person>(e =>
        {
            e.ToTable("Person", "dbo");
            e.Property(p => p.Username).HasColumnName("username");
            e.Property(p => p.Password_Hash).HasColumnName("password_hash");
        });
        var priorityConverter = new ValueConverter<TicketPriority, string>(
            v => v.ToString().ToLower(),
            v => Enum.Parse<TicketPriority>(v, true)
        );
        var typeConverter = new ValueConverter<TicketType, string>(
            v => v.ToString().ToLower(),
            v => Enum.Parse<TicketType>(v, true)
        );
        var statusConverter = new ValueConverter<TicketStatus, string>(
            v => v.ToString().ToLower(),
            v => Enum.Parse<TicketStatus>(v, true)
        );
        modelBuilder.Entity<Ticket>(e =>
        {
            e.ToTable("Ticket", "dbo");
            e.Property(t => t.Priority).HasColumnName("priority").HasConversion(priorityConverter);
            e.Property(t => t.Type).HasColumnName("type").HasConversion(typeConverter);
            e.Property(t => t.Status).HasColumnName("status").HasConversion(statusConverter);
            e.Property(t => t.CreatedById).HasColumnName("created_by");
            e.Property(t => t.UpdatedById).HasColumnName("updated_by");
            e.HasOne(t => t.CreatedBy).WithMany().HasForeignKey(t => t.CreatedById);
            e.HasOne(t => t.UpdatedBy).WithMany().HasForeignKey(t => t.UpdatedById);
        });
        modelBuilder.Entity<TicketUpdate>(e =>
        {
            e.ToTable("TicketUpdate", "dbo");
            e.Property(u => u.Id).HasColumnName("id");
            e.Property(u => u.TicketId).HasColumnName("ticket_id");
            e.Property(u => u.UpdatedById).HasColumnName("updated_by");
            e.Property(u => u.UpdatedAt).HasColumnName("updated_at");
            e.Property(u => u.Note).HasColumnName("note");
            e.HasOne(u => u.Ticket).WithMany().HasForeignKey(u => u.TicketId);
            e.HasOne(u => u.UpdatedBy).WithMany().HasForeignKey(u => u.UpdatedById);
        });

    }
}