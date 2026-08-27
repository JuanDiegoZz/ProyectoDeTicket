using Microsoft.EntityFrameworkCore;
using TicketsApp.Domain.Entities;
using TicketsApp.Domain.Enums;

namespace TicketsApp.Infrastructure.Data;

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

        // 1. Usuarios
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("usuarios");
            entity.HasKey(u => u.Id).HasName("pk_usuarios");
            entity.HasIndex(u => u.Email).IsUnique().HasDatabaseName("ix_usuarios_email");

            entity.Property(u => u.Rol).HasConversion<string>();
            entity.Property(u => u.TipoSolicitante).HasConversion<string>();
        });

        // 2. Categorías
        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.ToTable("categorias");
            entity.HasKey(c => c.Id).HasName("pk_categorias");
        });

        // 3. Ubicaciones
        modelBuilder.Entity<Ubicacion>(entity =>
        {
            entity.ToTable("ubicaciones");
            entity.HasKey(u => u.Id).HasName("pk_ubicaciones");
        });

        // 4. Tickets
        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.ToTable("tickets");
            entity.HasKey(t => t.Id).HasName("pk_tickets");

            entity.Property(t => t.Prioridad).HasConversion<string>();
            entity.Property(t => t.Estado).HasConversion<string>();

            entity.HasOne(t => t.Solicitante)
                .WithMany(u => u.TicketsReportados)
                .HasForeignKey(t => t.SolicitanteId)
                .HasConstraintName("fk_tickets_usuarios_solicitante_id")
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(t => t.TecnicoAsignado)
                .WithMany(u => u.TicketsAsignados)
                .HasForeignKey(t => t.TecnicoAsignadoId)
                .HasConstraintName("fk_tickets_usuarios_tecnico_asignado_id")
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(t => t.Categoria)
                .WithMany(c => c.Tickets)
                .HasForeignKey(t => t.CategoriaId)
                .HasConstraintName("fk_tickets_categorias_categoria_id")
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(t => t.Ubicacion)
                .WithMany(u => u.Tickets)
                .HasForeignKey(t => t.UbicacionId)
                .HasConstraintName("fk_tickets_ubicaciones_ubicacion_id")
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(t => t.Estado).HasDatabaseName("ix_tickets_estado");
            entity.HasIndex(t => t.Prioridad).HasDatabaseName("ix_tickets_prioridad");
            entity.HasIndex(t => t.FechaCreacion).HasDatabaseName("ix_tickets_fecha_creacion");
        });

        // 5. Notas de Ticket
        modelBuilder.Entity<NotaTicket>(entity =>
        {
            entity.ToTable("notas_ticket");
            entity.HasKey(n => n.Id).HasName("pk_notas_ticket");

            entity.HasOne(n => n.Ticket)
                .WithMany(t => t.Notas)
                .HasForeignKey(n => n.TicketId)
                .HasConstraintName("fk_notas_ticket_tickets_ticket_id")
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(n => n.Usuario)
                .WithMany(u => u.Notas)
                .HasForeignKey(n => n.UsuarioId)
                .HasConstraintName("fk_notas_ticket_usuarios_usuario_id")
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(n => n.TicketId).HasDatabaseName("ix_notas_ticket_ticket_id");
        });

        // Convención Automática de Nombres snake_case
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var key in entity.GetKeys())
            {
                if (key.GetName() != null) key.SetName(ToSnakeCase(key.GetName()!));
            }
            foreach (var foreignKey in entity.GetForeignKeys())
            {
                if (foreignKey.GetConstraintName() != null) foreignKey.SetConstraintName(ToSnakeCase(foreignKey.GetConstraintName()!));
            }
            foreach (var index in entity.GetIndexes())
            {
                if (index.GetDatabaseName() != null) index.SetDatabaseName(ToSnakeCase(index.GetDatabaseName()!));
            }
        }

        SeedData(modelBuilder);
    }

    private static string ToSnakeCase(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        var startUnderscores = System.Text.RegularExpressions.Regex.Match(input, @"^_+");
        return startUnderscores + System.Text.RegularExpressions.Regex.Replace(input, @"([a-z0-9])([A-Z])", "$1_$2").ToLowerInvariant();
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Categoria>().HasData(
            new Categoria { Id = 1, Nombre = "Hardware", Descripcion = "Fallas físicas en equipos de cómputo, impresoras o periféricos", Activo = true },
            new Categoria { Id = 2, Nombre = "Software", Descripcion = "Instalación de programas, errores de SO o licencias", Activo = true },
            new Categoria { Id = 3, Nombre = "Red/Internet", Descripcion = "Problemas de conexión Wi-Fi, Ethernet o acceso a servicios de red", Activo = true },
            new Categoria { Id = 4, Nombre = "Proyectores/Audiovisual", Descripcion = "Cañones proyectores, cables HDMI, sistemas de audio en aulas", Activo = true },
            new Categoria { Id = 5, Nombre = "Cuentas Institucionales", Descripcion = "Acceso a correos, restablecimiento de contraseñas de plataforma", Activo = true }
        );

        modelBuilder.Entity<Ubicacion>().HasData(
            new Ubicacion { Id = 1, Nombre = "E1", Activo = true },
            new Ubicacion { Id = 2, Nombre = "E2", Activo = true },
            new Ubicacion { Id = 3, Nombre = "E3", Activo = true },
            new Ubicacion { Id = 4, Nombre = "E4", Activo = true },
            new Ubicacion { Id = 5, Nombre = "Gym", Activo = true },
            new Ubicacion { Id = 6, Nombre = "Salón de Música", Activo = true }
        );

        modelBuilder.Entity<Usuario>().HasData(
            new Usuario
            {
                Id = 1,
                NombreCompleto = "Administrador TI",
                Email = "admin@monclova.tecnm.mx",
                PasswordHash = "$2a$11$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy",
                Rol = RolUsuario.Administrador,
                TipoSolicitante = TipoSolicitante.Profesor,
                Activo = true,
                FechaRegistro = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
