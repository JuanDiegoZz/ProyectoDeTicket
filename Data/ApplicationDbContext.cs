using Microsoft.EntityFrameworkCore;
using TicketsApp.Models;

namespace TicketsApp.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Ubicacion> Ubicaciones => Set<Ubicacion>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<NotaTicket> NotasTicket => Set<NotaTicket>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Índices únicos
        modelBuilder.Entity<Usuario>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // Relaciones de Ticket con Usuario (Solicitante y Técnico)
        modelBuilder.Entity<Ticket>()
            .HasOne(t => t.Solicitante)
            .WithMany(u => u.TicketsReportados)
            .HasForeignKey(t => t.SolicitanteId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Ticket>()
            .HasOne(t => t.TecnicoAsignado)
            .WithMany(u => u.TicketsAsignados)
            .HasForeignKey(t => t.TecnicoAsignadoId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Ticket>()
            .HasOne(t => t.Categoria)
            .WithMany(c => c.Tickets)
            .HasForeignKey(t => t.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Ticket>()
            .HasOne(t => t.Ubicacion)
            .WithMany(u => u.Tickets)
            .HasForeignKey(t => t.UbicacionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<NotaTicket>()
            .HasOne(n => n.Ticket)
            .WithMany(t => t.Notas)
            .HasForeignKey(n => n.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<NotaTicket>()
            .HasOne(n => n.Usuario)
            .WithMany(u => u.Notas)
            .HasForeignKey(n => n.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        // Conversión de Enums a String para mayor legibilidad en PostgreSQL
        modelBuilder.Entity<Usuario>()
            .Property(u => u.Rol)
            .HasConversion<string>();

        modelBuilder.Entity<Usuario>()
            .Property(u => u.TipoSolicitante)
            .HasConversion<string>();

        modelBuilder.Entity<Ticket>()
            .Property(t => t.Prioridad)
            .HasConversion<string>();

        modelBuilder.Entity<Ticket>()
            .Property(t => t.Estado)
            .HasConversion<string>();

        // Seed Data para Catálogos Base y Usuario Admin por defecto
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        // Categorías Requeridas
        modelBuilder.Entity<Categoria>().HasData(
            new Categoria { Id = 1, Nombre = "Hardware", Descripcion = "Fallas físicas en equipos de cómputo, impresoras o periféricos", Activo = true },
            new Categoria { Id = 2, Nombre = "Software", Descripcion = "Instalación de programas, errores de SO o licencias", Activo = true },
            new Categoria { Id = 3, Nombre = "Red/Internet", Descripcion = "Problemas de conexión Wi-Fi, Ethernet o acceso a servicios de red", Activo = true },
            new Categoria { Id = 4, Nombre = "Proyectores/Audiovisual", Descripcion = "Cañones proyectores, cables HDMI, sistemas de audio en aulas", Activo = true },
            new Categoria { Id = 5, Nombre = "Cuentas Institucionales", Descripcion = "Acceso a correos, restablecimiento de contraseñas de plataforma", Activo = true }
        );

        // Ubicaciones Requeridas
        modelBuilder.Entity<Ubicacion>().HasData(
            new Ubicacion { Id = 1, Nombre = "E1", Activo = true },
            new Ubicacion { Id = 2, Nombre = "E2", Activo = true },
            new Ubicacion { Id = 3, Nombre = "E3", Activo = true },
            new Ubicacion { Id = 4, Nombre = "E4", Activo = true },
            new Ubicacion { Id = 5, Nombre = "Gym", Activo = true },
            new Ubicacion { Id = 6, Nombre = "Salón de Música", Activo = true }
        );

        // Administrador por defecto (Contraseña inicial: Admin123!)
        modelBuilder.Entity<Usuario>().HasData(
            new Usuario
            {
                Id = 1,
                NombreCompleto = "Administrador TI",
                Email = "admin@monclova.tecnm.mx",
                PasswordHash = "$2a$11$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy", // Hash de 'Admin123!'
                Rol = RolUsuario.Administrador,
                TipoSolicitante = TipoSolicitante.Profesor,
                Activo = true,
                FechaRegistro = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
