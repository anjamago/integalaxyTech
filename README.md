# IntergalaxyTech — Backend API (.NET 8)

API REST construida con **.NET 8**, organizada en **arquitectura por capas** (API → Application → Domain → Infrastructure). Desplegada en Azure App Service vía GitHub Actions + Docker.

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
k8s/                               # Manifiestos Kubernetes
.github/workflows/                 # CI/CD con GitHub Actions → Azure
```

### Por qué esta división

Cada capa tiene una sola razón para cambiar:

- **Domain** solo contiene el modelo de negocio. No sabe nada de bases de datos ni HTTP.
- **Application** coordina los casos de uso. Define qué tiene que pasar; no cómo se persiste.
- **Infrastructure** implementa los contratos que define Application. Si mañana cambias SQLite por SQL Server, solo tocas esta capa.
- **API** recibe el HTTP y retorna los status codes correctos. Nada más.

Los controladores no instancian servicios directamente — dependen de interfaces inyectadas por constructor. Esto es lo que hace que los tests unitarios funcionen sin levantar la base de datos.

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
docker-compose up --build
```

La API queda disponible en `https://localhost:7xxx` (el puerto exacto aparece en la consola).

```bash
# Tests
dotnet test
```

---

## Endpoints principales

| Método | Ruta | Descripción |
|---|---|---|
| `POST` | `/api/solicitudes` | Crea una solicitud |
| `GET` | `/api/solicitudes/{id}` | Consulta una solicitud por ID |
| `GET` | `/api/personajes` | Lista personajes (fuente: Rick & Morty API) |

Todas las respuestas siguen el wrapper `ApiResponse<T>` con estructura uniforme. Los errores de validación retornan `HTTP 400` con detalle en JSON — manejados por el `GlobalExceptionMiddleware`, no con lógica dispersa en los controladores.

---

## Validaciones

Las reglas de entrada están centralizadas en FluentValidation. Ejemplo para crear una solicitud:

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

El proyecto incluye un análisis de migración de un fragmento legado (`SolicitudForm.aspx.cs`) a .NET 8. Los problemas del código original:

- **SQL Injection** por concatenación directa de inputs en queries
- **Credenciales hardcodeadas** en el código fuente (`Password=admin123`)
- **Violación de SRP**: un solo archivo mezclaba renderizado, base de datos, lógica de negocio y manejo de sesión
- **Session en memoria del servidor**: impide escalar horizontalmente en un clúster cloud

La versión migrada usa EF Core (sin SQL crudo), configuración por variables de entorno, y es stateless por diseño.

---

## CI/CD

El pipeline en `.github/workflows/` hace build, corre los tests, construye la imagen Docker y despliega a Azure App Service. El despliegue solo ocurre si los 6 tests pasan.

---

## Configuración local

Las cadenas de conexión y secretos van en `appsettings.Development.json` (ignorado por `.gitignore`) o en variables de entorno. No hay credenciales hardcodeadas en el código.

---

## 10. Diseño y Arquitectura de Cloud (Azure)

Referencia de estructura en la nube si estuviéramos migrando esto a Microsoft Azure integral.

| Necesidad del sistema | Servicio Azure que usarías (y razón) |
| --- | --- |
| Hospedar la API .NET 8 | **Azure App Service (Linux Web App)**: Plataforma PaaS robusta que soporta despliegues ZIP de .NET nativos como nuestro CI/CD. Otorga envoltura para certificados, auto-escalado horizontal, health checks e inyección de secretos vía Envs. |
| Base de datos relacional | **Azure SQL Database**: DbaaS de primer nivel con respaldos automáticos, encriptación en reposo y altísima disponibilidad; el aliado ideal para EF Core dada la predictibilidad de un sistema de solicitudes. |
| Almacenar archivos o reportes PDF generados | **Azure Blob Storage**: Almacenamiento "object-based" de costo insignificante, optimizado para archivos estáticos y capaz de proveer URLs inmutables o temporales (SAS tokens) para descargar certificados o adjuntos de los roles sin cargar nuestra capa de aplicación. |
| Exponer y versionar la API hacia terceros | **Azure API Management (APIM)**: Fija la fachada. Nos va a permitir abstraer y ocultar nuestras IPs reales, inyectar "Policies" unificadas (Throttling / rate-limiting) y llevar un versionado semántico (v1, v2) escalable. |
| Ejecutar tareas programadas o eventos async | **Azure Functions (Serverless)**: Configurado con un *TimerTrigger* o encolamiento en event-bus, es el terreno perfecto y ultra-barato (Cobro por milisegundo) para ejecutar la sincronización pesada con la API de "Rick & Morty" en background en lugar de colgar un "POST". |

---

## 11. Cuestionario de Liderazgo Técnico

**¿Cómo planificarías la migración completa del sistema legado en etapas graduales?**
> Utilizaría puramente el **Patrón Strangler Fig (Higuera Estranguladora)**. Iniciar migrando únicamente los lados de menor impacto o contexto de Solo-Lectura (por ejemplo, el CRUD de los *Personajes*). Se dejaría el sistema legado detrás de un API Gateway y se irían estrangulando módulos paso a paso enviándole sus peticiones de red al nuevo backend, hasta que finalmente todas las vistas del monolito y su base de datos original queden deprecadas.

**¿Qué estrategia usarías si el sistema legado debe operar en paralelo durante la transición?**
> Al estar en paralelo, el principal peligro es el desdoblamiento de los datos y las dependencias (La base de datos original en WebForms y la limpia de Azure SQL de .NET 8). Pondría una Fachada para enrutar el tráfico inteligente, y usaría un enfoque de base de datos de "Lectura compartida" o **Change Data Capture (CDC)** con Azure Service Bus. Si algo se crea en Web Forms, detona un evento que avisa a nuestro EF Core de la nueva API, manteniendo consistencia basal a lo largo del solapamiento para evitar aislamiento de datos.

**¿Cómo organizarías a un equipo de 3 desarrolladores para este módulo? (roles, code reviews, ramas Git)**
> Implementaría la metodología **Git Flow Ligero** (o Trunk Based).
> *   **Ramas**: La rama `main` de Producción estará siempre bloqueada de commits directos. Todo nace de ramas `feature/` o `bugfix/`.
> *   **Roles**: Yo como Lead asumo la titularidad DevOps, CI/CD y diagramación (Arquitectura), validando pull requests. El *Dev A* trabaja el contexto de Dominio, Interfaces y Logica Base. El *Dev B* opera la Infraestructura (EF Core y conectividad HTTP Externa).
> *   **Code Reviews**: Nadie asila. Se obligaría 1 un "Approved" por otro peer antes del merge vía PR en GitHub y pasar las barreras en las GitHub Actions de Build y Test Limpios. 

---

## 12. Transparencia: Herramientas de IA utilizadas
Conforme a los estatutos de la prueba técnica:
*   **Antigravity (Deepmind AI Assistant)**: Utilizada bajo supervisión para agilizar *Scaffolding*, revisión cruzada del Patrón de Clean Architecture y conformación de flujos condicionales de control (La Máquina de Estados para solicitudes) buscando las mejores prácticas referenciadas por Microsoft.