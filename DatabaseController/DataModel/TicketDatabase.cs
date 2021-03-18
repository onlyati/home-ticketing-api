using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using DatabaseController.Model;

#nullable disable

namespace DatabaseController.DataModel
{
    public partial class TicketDatabase : DbContext
    {
        public TicketDatabase(DbContextOptions<TicketDatabase> options)
            : base(options)
        {
        }

        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<Log> Logs { get; set; }
        public virtual DbSet<System> Systems { get; set; }
        public virtual DbSet<Ticket> Tickets { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Usercategory> Usercategories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "en_US.UTF-8");

            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("categories");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("name");

                entity.Property(e => e.SystemId).HasColumnName("system_id");

                entity.HasOne(d => d.System)
                    .WithMany(p => p.Categories)
                    .HasForeignKey(d => d.SystemId)
                    .HasConstraintName("categories_system_id_fkey");
            });

            modelBuilder.Entity<Log>(entity =>
            {
                entity.ToTable("logs");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Details).HasColumnName("details");

                entity.Property(e => e.Summary)
                    .IsRequired()
                    .HasMaxLength(120)
                    .HasColumnName("summary");

                entity.Property(e => e.TicketId).HasColumnName("ticket_id");

                entity.Property(e => e.Time)
                    .HasColumnName("time")
                    .HasDefaultValueSql("now()");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.HasOne(d => d.Ticket)
                    .WithMany(p => p.Logs)
                    .HasForeignKey(d => d.TicketId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("logs_ticket_id_fkey");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Logs)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("logs_user_id_fkey");
            });

            modelBuilder.Entity<System>(entity =>
            {
                entity.ToTable("systems");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("name");
            });

            modelBuilder.Entity<Ticket>(entity =>
            {
                entity.ToTable("tickets");

                entity.HasIndex(e => e.Status, "ticket_status_idx");

                entity.HasIndex(e => e.Reference, "tickets_reference_idx");

                entity.HasIndex(e => e.Title, "tickets_title_idx");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CategoryId).HasColumnName("category_id");

                entity.Property(e => e.Reference)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("reference");

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasMaxLength(10)
                    .HasColumnName("status");

                entity.Property(e => e.SystemId).HasColumnName("system_id");

                entity.Property(e => e.Time)
                    .HasColumnType("timestamp with time zone")
                    .HasColumnName("time")
                    .HasDefaultValueSql("now()");

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(120)
                    .HasColumnName("title");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.HasOne(d => d.Category)
                    .WithMany(p => p.Tickets)
                    .HasForeignKey(d => d.CategoryId)
                    .HasConstraintName("tickets_category_id_fkey");

                entity.HasOne(d => d.System)
                    .WithMany(p => p.Tickets)
                    .HasForeignKey(d => d.SystemId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("tickets_system_id_fkey");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Tickets)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("tickets_user_id_fkey");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Email)
                    .HasMaxLength(120)
                    .HasColumnName("email");

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasMaxLength(512)
                    .HasColumnName("password");

                entity.Property(e => e.Username)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("username");

                entity.Property(e => e.Role)
                    .HasMaxLength(10)
                    .HasColumnName("role")
                    .HasConversion
                    (
                        v => v.ToString(),
                        v => (UserRole)Enum.Parse(typeof(UserRole), v)
                    );
            });

            modelBuilder.Entity<Usercategory>(entity =>
            {
                entity.ToTable("usercategories");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasDefaultValueSql("nextval('usercategiryrelation_id_seq'::regclass)");

                entity.Property(e => e.CategoryId).HasColumnName("category_id");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.HasOne(d => d.Category)
                    .WithMany(p => p.Usercategories)
                    .HasForeignKey(d => d.CategoryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("usercategiryrelation_category_id_fkey");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Usercategories)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("usercategiryrelation_user_id_fkey");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
