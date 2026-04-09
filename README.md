# IntergalaxyTech — Backend API (.NET 8)

API REST construida con **.NET 8**, organizada en **arquitectura limpia** (API / Application / Domain / Infrastructure).

---

## Estructura del proyecto

```
src/
├── IntergalaxyTech.API            # Controladores HTTP, Middleware, Program.cs
├── IntergalaxyTech.Application    # Casos de uso, DTOs, validaciones (FluentValidation)
├── IntergalaxyTech.Domain         # Entidades y enums de negocio
└── IntergalaxyTech.Infrastructure # EF Core (SQLite), consumo de Rick & Morty API externa
tests/
└── IntergalaxyTech.Tests          # Tests unitarios con xUnit (6 tests)
deploy/                            # Configuraciones de Docker y Docker compose
.github/workflows/                 # CI/CD con GitHub Actions → Azure
```

### Por qué esta división

Cada capa tiene una sola razón para cambiar:

- **Domain** solo contiene el modelo de negocio. 
- **Application** coordina los casos de uso, Define qué tiene que pasar.
- **Infrastructure** implementa los contratos que define Application. Si mañana cambias SQLite por SQL Server, solo se toca esta capa.
- **API** recibe las solicitudes HTTP y retorna los status correctos.

Los controladores no instancian servicios directamente — dependen de interfaces inyectadas por constructor. Esto lo que hace es que los tests unitarios funcionen sin levantar la base de datos.

---

## Stack técnico

| Componente | Tecnología |
|---|---|
| Runtime | .NET 8 |
| ORM | Entity Framework Core + SQLite |
| Validación | FluentValidation |
| Tests | xUnit |
| CI/CD | GitHub Actions → Azure App Service |
| Contenedor | Docker + docker-compose |

---

## Cómo correr el proyecto

**Prerequisitos:** .NET 8 SDK, Docker (opcional)

```bash
# Clonar
git clone https://github.com/anjamago/integalaxyTech.git
cd integalaxyTech

# Correr con dotnet
cd src/IntergalaxyTech.API
dotnet run

# O con Docker Compose
docker compose -f deploy/docker-compose.yml up --build
```

La API queda disponible en `https://localhost:5000` 
El swagger queda disponible en `http://localhost:5000/swagger/index.html` 

```bash
# Tests
dotnet test
```

---

## Endpoints

### Personajes

| Método | Ruta | Descripción |
|---|---|---|
| `POST` | `/api/personajes/importar` | Importa personajes desde la Rick & Morty API a la BD local |
| `GET` | `/api/personajes` | Listado paginado (filtros: `nombre`, `estado`) |
| `GET` | `/api/personajes/{id}` | Detalle de un personaje importado |

### Solicitudes

| Método | Ruta | Descripción |
|---|---|---|
| `POST` | `/api/solicitudes` | Crea una solicitud (personaje, solicitante, evento, fecha) |
| `GET` | `/api/solicitudes` | Listado con filtros opcionales (`estado`, `solicitante`) |
| `GET` | `/api/solicitudes/{id}` | Detalle de una solicitud |
| `PATCH` | `/api/solicitudes/{id}/estado` | Cambia el estado con validación de transición |

### Reportes y salud

| Método | Ruta | Descripción |
|---|---|---|
| `GET` | `/api/reportes/solicitudes-resumen` | Totales por estado y personaje más solicitado |
| `GET` | `/health` | Health check requerido por Azure App Service |

#### Transiciones de estado válidas

```
Pendiente  →  EnProceso  →  Aprobada
Pendiente  →  Rechazada
EnProceso  →  Rechazada
```

Cualquier transición fuera de ese diagrama retorna `HTTP 422`.

Todas las respuestas siguen el wrapper `ApiResponse<T>`. Los errores de validación retornan `HTTP 400` con detalle en JSON, manejados por `GlobalExceptionMiddleware`.

---

## Migraciones EF Core

```bash
# Crear una migración nueva
dotnet ef migrations add NombreMigracion \
  --project src/IntergalaxyTech.Infrastructure \
  --startup-project src/IntergalaxyTech.API

# Aplicar migraciones a la base de datos
dotnet ef database update \
  --project src/IntergalaxyTech.Infrastructure \
  --startup-project src/IntergalaxyTech.API

# Revertir a una migración anterior
dotnet ef database update NombreMigracionAnterior \
  --project src/IntergalaxyTech.Infrastructure \
  --startup-project src/IntergalaxyTech.API
```

Con `docker-compose up`, las migraciones se aplican automáticamente al arrancar la API.

---

## Configuración

### Local (`appsettings.Development.json`)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=intergalaxy.db"
  }
}
```

Este archivo está en `.gitignore`. No hay credenciales en el código fuente.

### Docker Compose

La connection string se inyecta como variable de entorno, simulando lo que sería un secreto en Azure Key Vault o App Service Configuration:

```yaml
environment:
  - ConnectionStrings__DefaultConnection=Data Source=/app/data/intergalaxy.db
```

### Azure SQL Database

Así se vería la configuración apuntando a producción en Azure. Este valor va en **Configuration  Connection strings** del App Service, no en `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=intergalaxy.database.windows.net;Database=IntergalaxyDB;Authentication=Active Directory Default;"
  }
}
```

---

## CI/CD

El pipeline en `.github/workflows/` hace build, corre los tests, construye la imagen Docker y despliega a Azure App Service. El despliegue solo ocurre si los 6 tests pasan.

---

## Validaciones

Las reglas de entrada están centralizadas en FluentValidation:

```csharp
public class CrearSolicitudValidator : AbstractValidator<CrearSolicitudDto>
{
    public CrearSolicitudValidator()
    {
        RuleFor(x => x.Solicitante)
            .NotEmpty().WithMessage("El solicitante es obligatorio")
            .MaximumLength(100);
        RuleFor(x => x.AccionRequerida)
            .NotEmpty().WithMessage("El evento/acción es obligatorio")
            .MaximumLength(500);
        RuleFor(x => x.PersonajeId)
            .GreaterThan(0).WithMessage("Seleccione un personaje válido.");
    }
}
```

Si la validación falla, el middleware lo convierte en un `400 Bad Request` formateado. El controlador no maneja ese caso — solo recibe el resultado cuando todo está bien.

---

## Migración desde Web Forms

El proyecto incluye un análisis del fragmento legado `SolicitudForm.aspx.cs`. Los cuatro problemas principales del código original:

- **SQL Injection** — los inputs del usuario se concatenan directamente en la query sin parametrizar
- **Credenciales hardcodeadas** — `Password=admin123` en texto plano en el código fuente
- **Violación de SRP** — un solo archivo mezclaba renderizado, acceso a BD, lógica de negocio y manejo de sesión
- **Session en memoria del servidor** — impide escalar horizontalmente; si la petición siguiente cae en otro nodo del clúster, la sesión no existe

La versión migrada usa EF Core (sin SQL crudo), configuración por variables de entorno, y es stateless por diseño.

---

## Diseño para Azure

| Necesidad | Servicio Azure | Razón |
|---|---|---|
| Hospedar la API .NET 8 | **App Service** (Linux) | escalamiento horizontal, health checks nativos, integración directa con Key Vault |
| Base de datos relacional | **Azure SQL Database** | permite que la base de datos sea elastica y compatibilidad total con mi app en .net|
| Almacenar archivos o reportes PDF | **Blob Storage**  | Permite almacenar documentos estaticos y recuperarlos via token, evitando crear volumnes inesearios en mi appService o k8s |
| Exponer y versionar la API hacia terceros | **API Management** | Permite administrar el trafico a mis recursos, alministra la autenticacion de mis servicios, permite el versionado `/v1/` `/v2/` sin tocar el código de la API |
| Tareas programadas o eventos async | **Azure Functions**  | funciones Serverless, cobro por ejecución, perfecto para sincronización en background sin bloquear el hilo |

---


## . Problemas de Diseño y Seguridad en el Código Legado

Al analizar el codigo anterior se identificaron inmediatamente las siguientes vulnerabilidades.

1. **Vulnerabilidad Crítica de SQL Injection:**
   El código construye una sentencias SQL concatenando los inputs del usuario directamente (`txtSolicitante.Text + "', '" + ...`). Esto permite que cualquier actor malicioso envíe comandos SQL destructivos en los campos de texto.
2. **Credenciales Hardcodeadas (Falta de Gestión de Secretos):**
   La cadena de conexión (`"Server=PROD-SERVER;...Password=admin123;"`) se declaró en variable. cada acceso a la base de datos genera una conexion nueva, exponiendo datos de conexion a la DB, esto causa que no exita una unica conexion con la DB.
3. **Violación Severa del SRP (Single Responsibility Principle):**
   La clase aglomera accesos de lectura/escritura SQL, renderizado desplegables HTML (`DataBind`), reglas de validación y control de sesiones HTTP.
4. **Estado Mutante y Anti-Cloud (Session Management):**
   Manejar las confirmaciones inyectando estado en la memoria del servidor (`Session["ultimaSolicitud"]`) impide escalar horizontalmente el sistema. En un clúster Cloud, la segunda petición puede caer en una máquina diferente perdiendo su sesión. 

5. **Validación Manual Frágil:**
   Hacer validaciones mediante simples `if (string.IsNullOrEmpty(...))` al interior del evento  clic causa una dispersión de la lógica de negocio, es propensa a errores por omisión a medida que la clase crece.

---


### A. Capa de Presentación (Controladores API)
El controlador aísla completamente al usuario de la lógica de negocio. inyecta abstracciones y se dedica unicamente a traducir el Protocolo HTTP.

```csharp
[ApiController]
[Route("api/[controller]")]
public class SolicitudesController : ControllerBase
{
    private readonly ISolicitudService _solicitudService;

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
Se refactorizando los contructores de `SqlCommand` se eliminan por medio de Repositorios Genéricos encapsulados sobre **Entity Framework Core**, mitigando por defecto el SQL Injection gracias a la abstracción de parámetros interna:

```csharp
public async Task AddAsync(T entity)
{
    await _context.Set<T>().AddAsync(entity);
    await _context.SaveChangesAsync();
}
```

---

## Liderazgo técnico

### ¿Cómo planificarías la migración completa del sistema legado en etapas graduales?

realizar una migracion de golpe o total es un riesgo muy alto aplicaria una estrategia de higuera, extrayendo servicios en paralelo
el negocio sigue operando sobre el sistema viejo mientras se construyes el nuevo.

Yo lo haría en tres fases con el **patrón Strangler Fig**:

**Fase 1 — Mapear y aislar** Inventario completo de los módulos del cuáles tienen más tráfico, cuáles tienen dependencias cruzadas, cuáles son candidatos a deprecarse directamente. El objetivo es un mapa de qué migrar, en qué orden y que no es necesario.

**Fase 2 — Migración módulo por módulo** Cada módulo se reimplementa como endpoint en la nueva API. El sistema viejo sigue vivo. Azure API Management rutea el tráfico, las rutas ya migradas van a la nueva API. Esto permite validar en producción sin cortar nada de golpe.

**Fase 3 — Corte final.** Cuando el 100% del tráfico está en la nueva API y el sistema legado no recibe peticiones por varias semanas consecutivas, se apaga. No antes.

### ¿Qué estrategia usarías si el sistema legado debe operar en paralelo durante la transición?

El detalle crítico es la base de datos compartida: durante la transición, ambos sistemas escriben a la misma BD. Eso obliga a mantener una unica fuente de verdad evitando asi la dublicidad de datos en el esquema mientras el legado sigue activo.


### ¿Cómo organizarías un equipo de 3 desarrolladores para este módulo?

Con 3 personas, una estructura plana funciona mejor, la división por foco técnico de los desarrolladores:

- **Desarrolladores Backend (2):** Responsables de la migración *end-to-end* de los flujos de trabajo, garantizando el cumplimiento de los Acuerdos de Nivel de Servicio (SLA). Tienen a su cargo el desarrollo de las nuevas API REST, la redacción técnica de los contratos de integración (ej. Swagger/OpenAPI) y el aseguramiento de la calidad del código mediante una cobertura de pruebas unitarias mínima del 80%.

- **Desarrollador Frontend (1):** Responsable de la construcción de las interfaces de usuario (vistas) y la integración de las APIs expuestas por el backend. Debe asegurar que el consumo de los servicios respete los contratos técnicos acordados, garantizando una implementación fluida y a cabalidad.


**Flujo de Trabajo y Canalización:** Mantenemos la rama `main` protegida contra *push* directo. Las nuevas integraciones se gestionan a través de ramas efímeras nombrandas bajo la convención de *Conventional Commits* (ej. `feat/importar-personajes`, `fix/estado-solicitud`). Para asegurar la calidad en la canalización (CI/CD), cada Pull Request requiere superar el pipeline automático (construcción y pruebas) y contar con al menos una aprobación de un par. Aplicamos una política de rotación activa en los *Code Reviews* para fomentar la propiedad colectiva del código; evitando así silos de información y garantizando que el conocimiento de componentes críticos se distribuya equitativamente en todo el equipo.

---

## Herramientas de IA utilizadas

- **Claude (Anthropic)** — redacción y revisión del README, generacion de plan de trabajo bajo indicaciones de aplicabilidad.
- **Antigraviti** — sugerencias inline en controladores y validadores
El código de negocio, la decisión de arquitectura y las respuestas de liderazgo son propias. Las herramientas se usaron para acelerar documentación y boilerplate, no para generar la solución.