using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace Absensiguru.Models;

public partial class ApplicationDbContext : DbContext
{
    public ApplicationDbContext()
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Absensi> Absensis { get; set; }

    public virtual DbSet<Guru> Gurus { get; set; }

    public virtual DbSet<Izin> Izins { get; set; }

    public virtual DbSet<Jabatan> Jabatans { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySql("server=localhost;port=3306;database=db_absensi_guru;user=root", Microsoft.EntityFrameworkCore.ServerVersion.Parse("10.4.32-mariadb"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_general_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Absensi>(entity =>
        {
            entity.HasKey(e => e.IdAbsensi).HasName("PRIMARY");

            entity.ToTable("absensi");

            entity.HasIndex(e => e.IdGuru, "fk_absensi_guru");

            entity.Property(e => e.IdAbsensi)
                .HasColumnType("int(11)")
                .HasColumnName("id_absensi");
            entity.Property(e => e.IdGuru)
                .HasColumnType("int(11)")
                .HasColumnName("id_guru");
            entity.Property(e => e.JamMasuk)
                .HasColumnType("time")
                .HasColumnName("jam_masuk");
            entity.Property(e => e.JamPulang)
                .HasColumnType("time")
                .HasColumnName("jam_pulang");
            entity.Property(e => e.Keterangan)
                .HasColumnType("text")
                .HasColumnName("keterangan");
            entity.Property(e => e.Status)
                .HasColumnType("enum('Hadir','Izin','Sakit','Alpha')")
                .HasColumnName("status");
            entity.Property(e => e.Tanggal).HasColumnName("tanggal");

            entity.HasOne(d => d.IdGuruNavigation).WithMany(p => p.Absensis)
                .HasForeignKey(d => d.IdGuru)
                .HasConstraintName("fk_absensi_guru");
        });

        modelBuilder.Entity<Guru>(entity =>
        {
            entity.HasKey(e => e.IdGuru).HasName("PRIMARY");

            entity.ToTable("guru");

            entity.HasIndex(e => e.IdJabatan, "fk_guru_jabatan");

            entity.HasIndex(e => e.Nip, "nip").IsUnique();

            entity.Property(e => e.IdGuru)
                .HasColumnType("int(11)")
                .HasColumnName("id_guru");
            entity.Property(e => e.Alamat)
                .HasColumnType("text")
                .HasColumnName("alamat");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.IdJabatan)
                .HasColumnType("int(11)")
                .HasColumnName("id_jabatan");
            entity.Property(e => e.JenisKelamin)
                .HasColumnType("enum('L','P')")
                .HasColumnName("jenis_kelamin");
            entity.Property(e => e.NamaGuru)
                .HasMaxLength(100)
                .HasColumnName("nama_guru");
            entity.Property(e => e.Nip)
                .HasMaxLength(30)
                .HasColumnName("nip");
            entity.Property(e => e.NoHp)
                .HasMaxLength(20)
                .HasColumnName("no_hp");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'Aktif'")
                .HasColumnType("enum('Aktif','Nonaktif')")
                .HasColumnName("status");

            entity.HasOne(d => d.IdJabatanNavigation).WithMany(p => p.Gurus)
                .HasForeignKey(d => d.IdJabatan)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_guru_jabatan");
        });

        modelBuilder.Entity<Izin>(entity =>
        {
            entity.HasKey(e => e.IdIzin).HasName("PRIMARY");

            entity.ToTable("izin");

            entity.HasIndex(e => e.IdGuru, "fk_izin_guru");

            entity.Property(e => e.IdIzin)
                .HasColumnType("int(11)")
                .HasColumnName("id_izin");
            entity.Property(e => e.Alasan)
                .HasColumnType("text")
                .HasColumnName("alasan");
            entity.Property(e => e.Bukti)
                .HasMaxLength(255)
                .HasColumnName("bukti");
            entity.Property(e => e.IdGuru)
                .HasColumnType("int(11)")
                .HasColumnName("id_guru");
            entity.Property(e => e.JenisIzin)
                .HasColumnType("enum('Izin','Sakit')")
                .HasColumnName("jenis_izin");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'Menunggu'")
                .HasColumnType("enum('Menunggu','Disetujui','Ditolak')")
                .HasColumnName("status");
            entity.Property(e => e.Tanggal).HasColumnName("tanggal");

            entity.HasOne(d => d.IdGuruNavigation).WithMany(p => p.Izins)
                .HasForeignKey(d => d.IdGuru)
                .HasConstraintName("fk_izin_guru");
        });

        modelBuilder.Entity<Jabatan>(entity =>
        {
            entity.HasKey(e => e.IdJabatan).HasName("PRIMARY");

            entity.ToTable("jabatan");

            entity.Property(e => e.IdJabatan)
                .HasColumnType("int(11)")
                .HasColumnName("id_jabatan");
            entity.Property(e => e.NamaJabatan)
                .HasMaxLength(100)
                .HasColumnName("nama_jabatan");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.IdUser).HasName("PRIMARY");

            entity.ToTable("users");

            entity.HasIndex(e => e.IdGuru, "id_guru").IsUnique();

            entity.HasIndex(e => e.Username, "username").IsUnique();

            entity.Property(e => e.IdUser)
                .HasColumnType("int(11)")
                .HasColumnName("id_user");
            entity.Property(e => e.IdGuru)
                .HasColumnType("int(11)")
                .HasColumnName("id_guru");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .HasColumnName("password");
            entity.Property(e => e.Role)
                .HasColumnType("enum('Admin','Guru','TU')")
                .HasColumnName("role");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .HasColumnName("username");

            entity.HasOne(d => d.IdGuruNavigation).WithOne(p => p.User)
                .HasForeignKey<User>(d => d.IdGuru)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_users_guru");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
