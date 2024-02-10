using System;
using System.Collections.Generic;
using KeyMan.Models;
using Microsoft.EntityFrameworkCore;

namespace LangaraCPSC.WebAPI.DbModels;

public partial class LCSDBContext : DbContext
{
    public LCSDBContext()
    {
    }

    public LCSDBContext(DbContextOptions<LCSDBContext> options)
        : base(options)
    {
    }
    
    public virtual DbSet<Exec> Execs { get; set; }

    public virtual DbSet<Execimage> Execimages { get; set; }

    public virtual DbSet<Execprofile> Execprofiles { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost;Database=testdb;Username=rishit;Password=tecsuskamboj93");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Exec>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("execs_pkey");

            entity.ToTable("execs");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Email)
                .HasMaxLength(64)
                .HasColumnName("email");
            entity.Property(e => e.Firstname)
                .HasMaxLength(64)
                .HasColumnName("firstname");
            entity.Property(e => e.Lastname)
                .HasMaxLength(64)
                .HasColumnName("lastname");
            entity.Property(e => e.Position).HasColumnName("position");
            entity.Property(e => e.Tenureend)
                .HasMaxLength(64)
                .HasColumnName("tenureend");
            entity.Property(e => e.Tenurestart)
                .HasMaxLength(64)
                .HasColumnName("tenurestart");
        });

        modelBuilder.Entity<Execimage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("execimages_pkey");

            entity.ToTable("execimages");

            entity.Property(e => e.Id)
                .HasMaxLength(64)
                .HasColumnName("id");
            entity.Property(e => e.Path)
                .HasMaxLength(512)
                .HasColumnName("path");
        });

        modelBuilder.Entity<Execprofile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("execprofiles_pkey");

            entity.ToTable("execprofiles");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(10000)
                .HasColumnName("description");
            entity.Property(e => e.Imageid)
                .HasMaxLength(36)
                .HasColumnName("imageid");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
