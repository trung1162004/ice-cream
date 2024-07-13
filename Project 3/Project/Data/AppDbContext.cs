using Project.Models.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Project.Models;
using PayPal.Api;

namespace Project.Data
{
    public class AppDbContext : IdentityDbContext<AppUser, AppRole, string>
    {
        public AppDbContext()
        {
        }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AboutU>(entity =>
            {
                entity.HasKey(e => e.RecipeId).HasName("PK__AboutUs__FDD988D0BEF8E03E");

                entity.Property(e => e.RecipeId).HasColumnName("RecipeID");
                entity.Property(e => e.Content).HasColumnType("text");
                entity.Property(e => e.Images)
                    .HasMaxLength(200)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Admin>(entity =>
            {
                entity.HasKey(e => e.AdminId).HasName("PK__Admin__719FE4E874AA666C");

                entity.ToTable("Admin");

                entity.Property(e => e.AdminId).HasColumnName("AdminID");
                entity.Property(e => e.Adminname)
                    .HasMaxLength(100)
                    .IsUnicode(false);
                entity.Property(e => e.Email)
                    .HasMaxLength(100)
                    .IsUnicode(false);
                entity.Property(e => e.Password)
                    .HasMaxLength(100)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Book>(entity =>
            {
                entity.HasKey(e => e.BookId).HasName("PK__Book__3DE0C227128A3422");

                entity.ToTable("Book");

                entity.Property(e => e.BookId).HasColumnName("BookID");
                entity.Property(e => e.Description).HasColumnType("text");
                entity.Property(e => e.Discount).HasColumnType("decimal(15, 2)");
                entity.Property(e => e.Images)
                    .HasMaxLength(100)
                    .IsUnicode(false);
                entity.Property(e => e.Name)
                    .HasMaxLength(100)
                    .IsUnicode(false);
                entity.Property(e => e.Price).HasColumnType("decimal(15, 2)");
                entity.Property(e => e.Slug)
                    .HasMaxLength(100)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<ContactU>(entity =>
            {
                entity.HasKey(e => e.ContactId).HasName("PK__ContactU__5C6625BB6D871CCE");

                entity.Property(e => e.ContactId).HasColumnName("ContactID");
                entity.Property(e => e.Content).HasColumnType("text");
                entity.Property(e => e.Email)
                    .HasMaxLength(200)
                    .IsUnicode(false);
                entity.Property(e => e.Name)
                    .HasMaxLength(200)
                    .IsUnicode(false);
                entity.Property(e => e.Phone)
                    .HasMaxLength(100)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Feedback>(entity =>
            {
                entity.HasKey(e => e.FeedbackId).HasName("PK__Feedback__6A4BEDF659A08FA6");

                entity.ToTable("Feedback");

                entity.Property(e => e.FeedbackId).HasColumnName("FeedbackID");
                entity.Property(e => e.Email)
                    .HasMaxLength(150)
                    .IsUnicode(false);
                entity.Property(e => e.FeedbackDate).HasColumnType("datetime");
                entity.Property(e => e.FeedbackText).HasColumnType("text");
                entity.Property(e => e.Name)
                    .HasMaxLength(200)
                    .IsUnicode(false)
                    .HasColumnName("name");
                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.HasOne(d => d.User).WithMany(p => p.Feedbacks)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK__Feedback__UserID__45F365D3");
            });

            modelBuilder.Entity<Orders>(entity =>
            {
                entity.HasKey(e => e.OrderId).HasName("PK__Order__C3905BAF258571DB");

                entity.ToTable("Order");

                entity.Property(e => e.OrderId).HasColumnName("OrderID");
                entity.Property(e => e.Address).HasColumnType("text");
                entity.Property(e => e.Email)
                    .HasMaxLength(50)
                    .IsUnicode(false);
                entity.Property(e => e.Name)
                    .HasMaxLength(50)
                    .IsUnicode(false);
                entity.Property(e => e.PaymentMethod)
                    .HasMaxLength(100)
                    .IsUnicode(false);
                entity.Property(e => e.Phone)
                    .HasMaxLength(50)
                    .IsUnicode(false);
                entity.Property(e => e.Status)
                    .HasMaxLength(50)
                    .IsUnicode(false);
               entity.HasOne(d => d.User).WithMany(p => p.Orders)
                      .HasForeignKey(d => d.UserId)
                      .HasConstraintName("FK__Order__UserID__160F4887");
            });

            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasKey(e => e.OrderitemId).HasName("PK__OrderIte__42A47DDF327A13F5");

                entity.ToTable("OrderItem");

                entity.Property(e => e.OrderitemId).HasColumnName("OrderitemID");
                entity.Property(e => e.Amount).HasColumnType("decimal(15, 2)");
                entity.Property(e => e.BookId).HasColumnName("BookID");
                entity.Property(e => e.OrderId).HasColumnName("OrderID");
                entity.Property(e => e.Total).HasColumnType("decimal(15, 2)");

                entity.HasOne(d => d.Book).WithMany(p => p.OrderItems)
                    .HasForeignKey(d => d.BookId)
                    .HasConstraintName("FK__OrderItem__BookI__4222D4EF");

                entity.HasOne(d => d.Order).WithMany(p => p.OrderItems)
                    .HasForeignKey(d => d.OrderId)
                    .HasConstraintName("FK__OrderItem__Order__4316F928");
            });


            modelBuilder.Entity<Recipe>(entity =>
            {
                entity.HasKey(e => e.RecipeId).HasName("PK__Recipe__FDD988D03B59E669");

                entity.ToTable("Recipe");

                entity.Property(e => e.RecipeId).HasColumnName("RecipeID");
                entity.Property(e => e.Images)
                    .HasMaxLength(200)
                    .IsUnicode(false);
                entity.Property(e => e.Ingredients).HasColumnType("text");
                entity.Property(e => e.IsEnabled)
                    .HasMaxLength(150)
                    .IsUnicode(false);
                entity.Property(e => e.Procedure).HasColumnType("text");
                entity.Property(e => e.RecipeName)
                    .HasMaxLength(300)
                    .IsUnicode(false);

                entity.HasOne(d => d.User).WithMany(p => p.Recipes)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK__Recipe__UserId__48CFD27E");
            });


            base.OnModelCreating(modelBuilder);
        }
        public DbSet<PaymentInfo> PaymentInfos { get; set; }
        public virtual DbSet<AboutU> AboutUs { get; set; }

        public virtual DbSet<Admin> Admins { get; set; }

        public virtual DbSet<Book> Books { get; set; }

        public virtual DbSet<ContactU> ContactUs { get; set; }

        public virtual DbSet<Feedback> Feedbacks { get; set; }

        public virtual DbSet<Orders> Orders { get; set; }

        public virtual DbSet<OrderItem> OrderItems { get; set; }

        public virtual DbSet<Recipe> Recipes { get; set; }

        public virtual DbSet<AppUser> User { get; set; }

    }
}