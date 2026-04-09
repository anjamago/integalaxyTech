# Ejercicio de Migración — Web Forms a .NET 8

A continuación, se detalla el análisis del fragmento heredado de `SolicitudForm.aspx.cs` (legacy Web Forms) y la demostración de cómo se reescribió la lógica equivalente en esta API .NET 8 aplicando *Clean Architecture*.

---

## 🛑 1. Problemas de Diseño y Seguridad en el Código Legado

Al analizar el *Code-Behind* original, se identificaron inmediatamente las siguientes vulnerabilidades y antipatrones críticos de cara a un ambiente Cloud:

1. **Vulnerabilidad Crítica de SQL Injection:**
   El código construye las sentencias SQL concatenando los inputs del usuario directamente (`txtSolicitante.Text + "', '" + ...`). Esto permite que cualquier actor malicioso envíe comandos SQL destructivos en los campos de texto, saltándose todas las barreras.
2. **Credenciales Hardcodeadas (Falta de Gestión de Secretos):**
   La cadena de conexión (`"Server=PROD-SERVER;...Password=admin123;"`) se declaró en texto plano. Esto impide pivotar de base de datos sin necesidad de recompilar y expone llaves maestras en los repositorios de versionamiento de código.
3. **Violación Severa del SRP (Single Responsibility Principle):**
   La clase aglomera accesos de lectura/escritura SQL, re-renderizado de menús desplegables HTML (`DataBind`), reglas de validación y control de sesiones HTTP. Convertir en pruebas unitarias este código es virtualmente imposible.
4. **Estado Mutante y Anti-Cloud (Session Management):**
   Manejar las confirmaciones inyectando estado en la memoria del servidor al vuelo (`Session["ultimaSolicitud"]`) impide escalar horizontalmente el sistema. En un clúster Cloud, la segunda petición puede caer en una máquina diferente perdiendo su sesión. Las API REST modernas deben ser estrictamente *Stateless*.
5. **Validación Manual Frágil:**
   Hacer validaciones mediante simples `if (string.IsNullOrEmpty(...))` al interior de los eventos de clic causa una dispersión de la lógica de negocio; es propensa a errores por omisión a medida que la clase crece.

---

## 🚀 2. Solución Equivalente en .NET 8 (Nuestra API)

Con la *Clean Architecture* implementada en el actual repositorio, la solución se estructuró dividiendo responsabilidades puras y abstractas:

### A. Capa de Presentación (Controladores API)
El controlador aísla completamente al usuario de la lógica de negocio. Es *Stateless*, inyecta abstracciones y se dedica unicamente a traducir el Protocolo HTTP.

```csharp
[ApiController]
[Route("api/[controller]")]
public class SolicitudesController : ControllerBase
{
    private readonly ISolicitudService _solicitudService;

    // Solo pedimos interfaces (Inyección de Dependencias)
    public SolicitudesController(ISolicitudService solicitudService)
    {
        _solicitudService = solicitudService;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<SolicitudDto>>> Create(CrearSolicitudDto dto)
    {
        var solicitud = await _solicitudService.CrearSolicitudAsync(dto);
        var response = ApiResponse<SolicitudDto>.Ok(solicitud, "Solicitud creada correctamente.");
        return CreatedAtAction(nameof(GetById), new { id = solicitud.Id }, response);
    }
}
```

### B. Capa de Aplicación (Validación y Casos de Uso)
Los antiguos `if()` fueron reemplazados por una central de validación usando **FluentValidation**.

```csharp
public class CrearSolicitudValidator : AbstractValidator<CrearSolicitudDto>
{
    public CrearSolicitudValidator()
    {
        RuleFor(x => x.Solicitante)
            .NotEmpty().WithMessage("El solicitante es obligatorio")
            .MaximumLength(100);
            
        RuleFor(x => x.AccionRequerida)
            .NotEmpty().WithMessage("El evento es obligatorio")
            .MaximumLength(500);
            
        RuleFor(x => x.PersonajeId)
            .GreaterThan(0).WithMessage("Seleccion un personaje válido.");
    }
}
```

El Servicio dirige todo el negocio determinista. Orquesta validadores y efectúa comportamientos sin siquiera saber si está operando sobre SQLite o sobre Azure SQL Database.

```csharp
public async Task<SolicitudDto> CrearSolicitudAsync(CrearSolicitudDto peticion)
{
    // Validación limpia. Lanza excepción atrapada por un Middleware Global (Devuelve Http 400 Json)
    await _validator.ValidateAndThrowAsync(peticion);

    var personaje = await _personajeRepository.GetByIdAsync(peticion.PersonajeId);
    if (personaje == null) throw new ArgumentException("Personaje no encontrado.");

    var solicitud = new Solicitud
    {
        Id = Guid.NewGuid(),
        PersonajeId = peticion.PersonajeId,
        Solicitante = peticion.Solicitante,
        AccionRequerida = peticion.AccionRequerida,
        Estado = EstadoSolicitud.Pendiente,
        FechaCreacion = DateTime.UtcNow
    };

    await _solicitudRepository.AddAsync(solicitud);
    
    return ToDto(solicitud);
}
```

### C. Capa de Infraestructura (Data)
Los engorrosos constructores de strings tipo `SqlCommand` se extirpan por medio de Repositorios Genéricos encapsulados sobre **Entity Framework Core**, mitigando por defecto el SQL Injection gracias a la abstracción de parámetros interna:

```csharp
public async Task AddAsync(T entity)
{
    await _context.Set<T>().AddAsync(entity);
    await _context.SaveChangesAsync();
}
```
