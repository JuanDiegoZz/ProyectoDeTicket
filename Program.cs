using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using TicketsApp.Application.Common.Interfaces;
using TicketsApp.Domain.Entities;
using TicketsApp.Domain.Enums;
using TicketsApp.Infrastructure.Data;
using TicketsApp.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Configurar DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Data Source=tickets_local.db";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (connectionString.Contains("Host=", StringComparison.OrdinalIgnoreCase) || connectionString.Contains("Port=", StringComparison.OrdinalIgnoreCase))
    {
        options.UseNpgsql(connectionString);
    }
    else
    {
        options.UseSqlite(connectionString);
    }
});

// 2. Inyección de Dependencias por Módulos
builder.Services.AddScoped<IEmailClassifierService, EmailClassifierService>();
builder.Services.AddScoped<ITicketService, TicketService>();
builder.Services.AddScoped<ICatalogoService, CatalogoService>();

// 3. Configurar Autenticación con Cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
    });

// 4. Servicios MVC
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Inicialización y Sembrado de Datos de Prueba
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.EnsureCreated();

        // 1. Asegurar Admin
        var admin = context.Usuarios.FirstOrDefault(u => u.Email == "admin@monclova.tecnm.mx");
        if (admin == null)
        {
            admin = new Usuario
            {
                NombreCompleto = "Administrador TI",
                Email = "admin@monclova.tecnm.mx",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                Rol = RolUsuario.Administrador,
                TipoSolicitante = TipoSolicitante.Profesor,
                Activo = true,
                FechaRegistro = DateTime.UtcNow.AddMonths(-2)
            };
            context.Usuarios.Add(admin);
            context.SaveChanges();
        }
        else
        {
            admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!");
            context.SaveChanges();
        }

        // 2. Sembrar Técnicos de prueba
        var tec1 = context.Usuarios.FirstOrDefault(u => u.Email == "carlos.tecnico@monclova.tecnm.mx");
        if (tec1 == null)
        {
            tec1 = new Usuario
            {
                NombreCompleto = "Ing. Carlos Mendoza (Redes)",
                Email = "carlos.tecnico@monclova.tecnm.mx",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Tecnico123!"),
                Rol = RolUsuario.Tecnico,
                TipoSolicitante = TipoSolicitante.Administrativo,
                Activo = true,
                FechaRegistro = DateTime.UtcNow.AddDays(-30)
            };
            context.Usuarios.Add(tec1);
        }

        var tec2 = context.Usuarios.FirstOrDefault(u => u.Email == "sofia.tecnico@monclova.tecnm.mx");
        if (tec2 == null)
        {
            tec2 = new Usuario
            {
                NombreCompleto = "Lic. Sofía Ramírez (Soporte)",
                Email = "sofia.tecnico@monclova.tecnm.mx",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Tecnico123!"),
                Rol = RolUsuario.Tecnico,
                TipoSolicitante = TipoSolicitante.Administrativo,
                Activo = true,
                FechaRegistro = DateTime.UtcNow.AddDays(-20)
            };
            context.Usuarios.Add(tec2);
        }

        // 3. Sembrar Usuarios (Alumnos y Docentes)
        var prof1 = context.Usuarios.FirstOrDefault(u => u.Email == "ruben.rr@monclova.tecnm.mx");
        if (prof1 == null)
        {
            prof1 = new Usuario
            {
                NombreCompleto = "Dr. Rubén Rodríguez (Docente)",
                Email = "ruben.rr@monclova.tecnm.mx",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Docente123!"),
                Rol = RolUsuario.Solicitante,
                TipoSolicitante = TipoSolicitante.Profesor,
                Activo = true,
                FechaRegistro = DateTime.UtcNow.AddDays(-40)
            };
            context.Usuarios.Add(prof1);
        }

        var prof2 = context.Usuarios.FirstOrDefault(u => u.Email == "patricia.hernandez@monclova.tecnm.mx");
        if (prof2 == null)
        {
            prof2 = new Usuario
            {
                NombreCompleto = "Mtra. Patricia Hernández",
                Email = "patricia.hernandez@monclova.tecnm.mx",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Docente123!"),
                Rol = RolUsuario.Solicitante,
                TipoSolicitante = TipoSolicitante.Profesor,
                Activo = true,
                FechaRegistro = DateTime.UtcNow.AddDays(-15)
            };
            context.Usuarios.Add(prof2);
        }

        var alum1 = context.Usuarios.FirstOrDefault(u => u.Email == "I22050319@monclova.tecnm.mx");
        if (alum1 == null)
        {
            alum1 = new Usuario
            {
                NombreCompleto = "Juan Daniel Martínez (Ing. Sistemas)",
                Email = "I22050319@monclova.tecnm.mx",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Alumno123!"),
                Rol = RolUsuario.Solicitante,
                TipoSolicitante = TipoSolicitante.Alumno,
                Activo = true,
                FechaRegistro = DateTime.UtcNow.AddDays(-25)
            };
            context.Usuarios.Add(alum1);
        }

        var alum2 = context.Usuarios.FirstOrDefault(u => u.Email == "G22050319@monclova.tecnm.mx");
        if (alum2 == null)
        {
            alum2 = new Usuario
            {
                NombreCompleto = "Andrea Morales (Ing. Gestión)",
                Email = "G22050319@monclova.tecnm.mx",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Alumno123!"),
                Rol = RolUsuario.Solicitante,
                TipoSolicitante = TipoSolicitante.Alumno,
                Activo = true,
                FechaRegistro = DateTime.UtcNow.AddDays(-10)
            };
            context.Usuarios.Add(alum2);
        }

        context.SaveChanges();

        // 4. Sembrar Tickets de Prueba
        if (!context.Tickets.Any())
        {
            var catHardware = context.Categorias.First(c => c.Nombre == "Hardware").Id;
            var catSoftware = context.Categorias.First(c => c.Nombre == "Software").Id;
            var catRed = context.Categorias.First(c => c.Nombre == "Red/Internet").Id;
            var catAudio = context.Categorias.First(c => c.Nombre == "Proyectores/Audiovisual").Id;
            var catCuentas = context.Categorias.First(c => c.Nombre == "Cuentas Institucionales").Id;

            var ubE1 = context.Ubicaciones.First(u => u.Nombre == "E1").Id;
            var ubE2 = context.Ubicaciones.First(u => u.Nombre == "E2").Id;
            var ubE3 = context.Ubicaciones.First(u => u.Nombre == "E3").Id;
            var ubE4 = context.Ubicaciones.First(u => u.Nombre == "E4").Id;
            var ubGym = context.Ubicaciones.First(u => u.Nombre == "Gym").Id;

            var ticketsPrueba = new List<Ticket>
            {
                new Ticket
                {
                    Titulo = "Proyector no da video en clase de Cálculo",
                    Descripcion = "El cañón Epson del aula enciende la lámpara pero muestra pantalla azul sin señal de HDMI.",
                    CategoriaId = catAudio,
                    UbicacionId = ubE1,
                    DetalleAula = "Aula 104 - Planta Baja",
                    Prioridad = PrioridadTicket.Alta,
                    Estado = EstadoTicket.EnProgreso,
                    SolicitanteId = prof1.Id,
                    TecnicoAsignadoId = tec1.Id,
                    FechaCreacion = DateTime.UtcNow.AddHours(-4)
                },
                new Ticket
                {
                    Titulo = "Sin conexión a internet en Laboratorio de Cómputo",
                    Descripcion = "Toda la fila de PCs número 3 perdió la conexión a la red cableada y Wi-Fi.",
                    CategoriaId = catRed,
                    UbicacionId = ubE2,
                    DetalleAula = "Laboratorio 3 - Sistemas",
                    Prioridad = PrioridadTicket.Alta,
                    Estado = EstadoTicket.Abierto,
                    SolicitanteId = prof2.Id,
                    TecnicoAsignadoId = null,
                    FechaCreacion = DateTime.UtcNow.AddHours(-2)
                },
                new Ticket
                {
                    Titulo = "Computadora 12 no inicia Windows (Pantalla Azul)",
                    Descripcion = "Al presionar el botón de encendido se queda en bucle de recuperación de disco.",
                    CategoriaId = catHardware,
                    UbicacionId = ubE3,
                    DetalleAula = "Centro de Cómputo E3",
                    Prioridad = PrioridadTicket.Normal,
                    Estado = EstadoTicket.Resuelto,
                    SolicitanteId = alum1.Id,
                    TecnicoAsignadoId = tec2.Id,
                    FechaCreacion = DateTime.UtcNow.AddDays(-2),
                    FechaResolucion = DateTime.UtcNow.AddDays(-1)
                },
                new Ticket
                {
                    Titulo = "Restablecimiento de contraseña de Moodle institucional",
                    Descripcion = "No me permite ingresar al campus virtual para entregar proyecto final.",
                    CategoriaId = catCuentas,
                    UbicacionId = ubE4,
                    DetalleAula = "Planta Alta",
                    Prioridad = PrioridadTicket.Normal,
                    Estado = EstadoTicket.EnProgreso,
                    SolicitanteId = alum2.Id,
                    TecnicoAsignadoId = tec2.Id,
                    FechaCreacion = DateTime.UtcNow.AddHours(-18)
                },
                new Ticket
                {
                    Titulo = "Instalación de Visual Studio 2022 y SQL Server",
                    Descripcion = "Se requiere preparar las 25 máquinas del aula para la materia de Taller de BD.",
                    CategoriaId = catSoftware,
                    UbicacionId = ubE1,
                    DetalleAula = "Aula 108",
                    Prioridad = PrioridadTicket.Alta,
                    Estado = EstadoTicket.Resuelto,
                    SolicitanteId = prof1.Id,
                    TecnicoAsignadoId = tec1.Id,
                    FechaCreacion = DateTime.UtcNow.AddDays(-5),
                    FechaResolucion = DateTime.UtcNow.AddDays(-3)
                },
                new Ticket
                {
                    Titulo = "Falla de sonido en bocinas del Gimnasio",
                    Descripcion = "El amplificador principal hace falso contacto para el evento cívico.",
                    CategoriaId = catAudio,
                    UbicacionId = ubGym,
                    DetalleAula = "Cabina de Audio Principal",
                    Prioridad = PrioridadTicket.Normal,
                    Estado = EstadoTicket.Abierto,
                    SolicitanteId = alum1.Id,
                    TecnicoAsignadoId = null,
                    FechaCreacion = DateTime.UtcNow.AddMinutes(-45)
                }
            };

            context.Tickets.AddRange(ticketsPrueba);
            context.SaveChanges();
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error al sembrar los datos de prueba.");
    }
}

// Configurar Pipeline HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Tickets}/{action=Index}/{id?}");

app.Run();
