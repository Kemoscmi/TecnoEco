using Microsoft.EntityFrameworkCore;
using Tecnologia.Domain;
namespace Tecnologia.Infrastructure;
public class TechnologyDbContext(DbContextOptions<TechnologyDbContext> options):DbContext(options){
 public DbSet<Ticket> Tickets=>Set<Ticket>();public DbSet<Activity> Activities=>Set<Activity>();
 protected override void OnModelCreating(ModelBuilder model){
  var t=model.Entity<Ticket>();t.ToTable("tecnologia_tickets");t.HasKey(x=>x.Id);t.Property(x=>x.Id).HasMaxLength(40);t.Property(x=>x.Title).HasMaxLength(140);t.Property(x=>x.Description).HasColumnType("text");
  foreach(var name in new[]{"Type","Origin","Priority","Status"})t.Property<string>(name).HasMaxLength(40);
  t.Property(x=>x.Requester).HasMaxLength(160);t.Property(x=>x.Assignee).HasMaxLength(160);
  var a=model.Entity<Activity>();a.ToTable("tecnologia_actividades");a.HasKey(x=>x.Id);a.Property(x=>x.Id).HasMaxLength(40);a.Property(x=>x.TicketId).HasMaxLength(40);a.Property(x=>x.Text).HasColumnType("text");a.Property(x=>x.Visibility).HasMaxLength(40);a.HasOne<Ticket>().WithMany().HasForeignKey(x=>x.TicketId).OnDelete(DeleteBehavior.Restrict);
 }
}
