# TaskManager

Gestor de tareas con **API REST** y **interfaz web**, hecho con **.NET 10**, ASP.NET Core, Entity Framework Core y SQL Server.

Las tareas tienen título, descripción, estado (pendiente, en curso o hecha), responsable y fecha límite. Se pueden crear, listar, ver, editar y borrar, filtrar por estado y por responsable, y paginar. Los responsables se pueden crear y eliminar.

## Contenido

- [Requisitos cubiertos](#requisitos-cubiertos)
- [Tecnologías](#tecnologías)
- [Arquitectura](#arquitectura)
- [Cómo ejecutarlo](#cómo-ejecutarlo)
- [Uso de la API](#uso-de-la-api)
- [Modelo de datos y validaciones](#modelo-de-datos-y-validaciones)
- [Decisiones que tomé](#decisiones-que-tomé)
- [Qué haría con más tiempo](#qué-haría-con-más-tiempo)
- [Cómo usé la IA](#cómo-usé-la-ia)

## Requisitos cubiertos

| Requisito | Dónde está |
|---|---|
| Crear, listar, ver, editar y borrar tareas | API `api/tareas` y páginas `/Tareas` |
| Filtros por estado y por responsable, con paginación | `GET api/tareas?estado=&responsableId=&pagina=&tamanoPagina=` y filtros del listado web |
| Validación de datos y errores con los códigos HTTP correctos | DataAnnotations en los DTOs, reglas de negocio en los servicios y manejador global de errores (`ProblemDetails`) |
| Base de datos SQL Server con migraciones | EF Core, proyecto `TaskManager.Infrastructure` |
| Tests | Unitarios (servicios y validación) e integración (API completa) en `tests/TaskManager.Tests` |
| Crear y eliminar responsables | API `api/responsables` y página `/Responsables` |
| Extra: autenticación | JWT en la API |

## Tecnologías

- .NET 10, ASP.NET Core (controllers para la API y Razor Pages para la web)
- Entity Framework Core 10 con SQL Server y migraciones
- Autenticación JWT (`Microsoft.AspNetCore.Authentication.JwtBearer`)
- xUnit, `WebApplicationFactory` y SQLite en memoria para los tests
- Bootstrap 5, Bootstrap Icons y CSS propio (con modo claro y oscuro)

## Arquitectura

```
TaskManager/
├── src/
│   ├── TaskManager.Domain          Entidades y enum (Tarea, Responsable, EstadoTarea)
│   ├── TaskManager.Application     DTOs, validaciones, servicios y excepciones de negocio
│   ├── TaskManager.Infrastructure  DbContext, mapeo de entidades, migraciones y datos de ejemplo
│   └── TaskManager.Web             API (controllers), JWT, manejador de errores y páginas Razor
└── tests/
    └── TaskManager.Tests           Tests unitarios y de integración
```

Regla de dependencias:

```
Web → Application, Infrastructure
Infrastructure → Application, Domain
Application → Domain
Domain → (nada)
```

Los servicios de `Application` usan el contexto de datos a través de la interfaz `IAppDbContext`, sin repositorios intermedios. Así `Application` no depende de EF Core con SQL Server ni de la web.

La web (Razor Pages) no accede a los servicios directamente: consume la **propia API por HTTP** mediante un cliente tipado (`IApiClient`).

## Cómo ejecutarlo

### Requisitos previos

- [SDK de .NET 10](https://dotnet.microsoft.com/download) (`dotnet --version` debe mostrar `10.x`)
- SQL Server (Express, Developer o LocalDB) y, opcionalmente, SSMS
- Herramienta de EF Core: `dotnet tool install --global dotnet-ef`
- Certificado de desarrollo HTTPS de confianza: `dotnet dev-certs https --trust`

### 1. Clonar

```bash
git clone https://github.com/TU_USUARIO/TaskManager.git
cd TaskManager
```

### 2. Cadena de conexión

Está en `src/TaskManager.Web/appsettings.json`. Por defecto usa autenticación de Windows contra la instancia local:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=TaskManagerDb;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

Si tu instancia tiene otro nombre (por ejemplo `localhost\\SQLEXPRESS`), cámbialo ahí.

### 3. Secretos (clave JWT y contraseña de la aplicación)

Estos dos valores **no están en el repositorio**. Se guardan con *user-secrets*:

```bash
dotnet user-secrets init --project src/TaskManager.Web
dotnet user-secrets set "Jwt:Key" "pon-aqui-una-clave-larga-de-al-menos-32-caracteres" --project src/TaskManager.Web
dotnet user-secrets set "Auth:Password" "UnaContraseñaSegura123" --project src/TaskManager.Web
```

- `Jwt:Key` necesita **mínimo 32 caracteres**.
- El usuario de la aplicación es `admin` (`Auth:Usuario` en `appsettings.json`) y su contraseña es la que acabas de definir.
- Si falta algún secreto, la aplicación no arranca y muestra un error de configuración indicando cuál.

### 4. Crear la base de datos

```bash
dotnet ef database update --project src/TaskManager.Infrastructure --startup-project src/TaskManager.Web
```

Crea la base `TaskManagerDb` con sus tablas y unos datos de ejemplo: 3 responsables y 12 tareas (suficientes para ver la paginación).

### 5. Ejecutar

```bash
dotnet run --project src/TaskManager.Web
```

Abre la dirección HTTPS que muestre la consola. La raíz redirige a `/Tareas`.

### 6. Tests

```bash
dotnet test
```

Los tests **no necesitan SQL Server ni secretos**: usan SQLite en memoria y su propia configuración.

### Solución de problemas

| Síntoma | Causa probable |
|---|---|
| La web muestra "No se pudo conectar con la API" | El certificado HTTPS de desarrollo no es de confianza: ejecuta `dotnet dev-certs https --trust` y reinicia |
| La web muestra "La web no está autorizada para llamar a la API" | Faltan o son incorrectos los *user-secrets* `Jwt:Key` o `Auth:Password` |
| La aplicación no arranca con un error de configuración | Falta un secreto o `Jwt:Key` tiene menos de 32 caracteres |
| `database update` no conecta | Revisa el nombre del servidor en la cadena de conexión (el mismo que usas en SSMS) |

Si la API estuviera en otra dirección distinta de la de la web, se puede indicar en `Api:BaseUrl` (en `appsettings.json`). Vacío significa "el mismo host y puerto de la web".

## Uso de la API

Todos los endpoints, salvo el login, requieren `Authorization: Bearer <token>`.

### Ejemplos con curl

El puerto es el que muestre la consola. `-k` se usa solo porque el certificado de desarrollo es autofirmado.

```bash
# 1) Obtener un token
curl -k -X POST https://localhost:PUERTO/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"usuario":"admin","password":"UnaContraseñaSegura123"}'

# 2) Listar tareas en curso del responsable 2 (página 1)
curl -k "https://localhost:PUERTO/api/tareas?estado=EnCurso&responsableId=2&pagina=1" \
  -H "Authorization: Bearer TU_TOKEN"

# 3) Crear una tarea
curl -k -X POST https://localhost:PUERTO/api/tareas \
  -H "Authorization: Bearer TU_TOKEN" -H "Content-Type: application/json" \
  -d '{"titulo":"Probar la API","descripcion":"Prueba manual","estado":"Pendiente","fechaLimite":"2026-12-30","responsableId":1}'
```

En Windows, usa `curl.exe` en PowerShell.

### Endpoints

| Método | Ruta | Descripción | Respuestas |
|---|---|---|---|
| POST | `/api/auth/login` | Devuelve un JWT (sin autenticación) | `200`, `400`, `401` |
| GET | `/api/tareas` | Lista con filtros y paginación | `200`, `400`, `401` |
| GET | `/api/tareas/{id}` | Detalle de una tarea | `200`, `401`, `404` |
| POST | `/api/tareas` | Crea una tarea | `201` (con `Location`), `400`, `401` |
| PUT | `/api/tareas/{id}` | Edita una tarea | `200`, `400`, `401`, `404` |
| DELETE | `/api/tareas/{id}` | Elimina una tarea | `204`, `401`, `404` |
| GET | `/api/responsables` | Lista de responsables (ordenados por nombre) | `200`, `401` |
| GET | `/api/responsables/{id}` | Detalle de un responsable | `200`, `401`, `404` |
| POST | `/api/responsables` | Crea un responsable | `201` (con `Location`), `400`, `401` |
| DELETE | `/api/responsables/{id}` | Elimina un responsable (si no tiene tareas) | `204`, `401`, `404`, `409` |

### Filtros y paginación

| Parámetro | Valores | Por defecto |
|---|---|---|
| `estado` | `Pendiente`, `EnCurso`, `Hecha` | sin filtrar |
| `responsableId` | id de un responsable | sin filtrar |
| `pagina` | 1 o mayor | 1 |
| `tamanoPagina` | de 1 a 50 | 10 |

Las tareas se ordenan por fecha límite ascendente y, si coinciden, por id (así la paginación no repite ni se salta filas). Respuesta:

```json
{
  "items": [ { "id": 3, "titulo": "...", "estado": "EnCurso", "fechaLimite": "2026-12-10", "responsableId": 2, "responsableNombre": "Luis Pérez" } ],
  "pagina": 1,
  "tamanoPagina": 10,
  "totalItems": 12,
  "totalPaginas": 2
}
```

El estado viaja siempre como **texto** (`"EnCurso"`), no como número.

### Errores

Los errores siguen el formato estándar `ProblemDetails` (RFC 7807):

| Situación | Código |
|---|---|
| Campos vacíos o fuera de longitud, fecha pasada al crear, estado inválido, paginación fuera de rango, JSON mal formado | `400` con el detalle por campo |
| El `responsableId` del cuerpo no existe | `400` |
| Sin token, token inválido o caducado, credenciales incorrectas | `401` |
| El `id` de la URL no existe | `404` |
| Borrar un responsable con tareas asignadas | `409` |
| Error inesperado | `500` genérico (el detalle solo queda en el log) |

Ejemplo de `400`:

```json
{
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": { "Titulo": ["El título es obligatorio."] }
}
```

## Modelo de datos y validaciones

**Tarea**

| Campo | Reglas |
|---|---|
| `titulo` | Obligatorio, de 3 a 200 caracteres |
| `descripcion` | Opcional, máximo 1000 caracteres |
| `estado` | `Pendiente` (por defecto), `EnCurso` o `Hecha` |
| `fechaLimite` | Obligatoria (solo fecha). **No puede estar en el pasado al crear**; al editar sí se permite |
| `responsableId` | Obligatorio y debe existir |

**Responsable**

| Campo | Reglas |
|---|---|
| `nombre` | Obligatorio, de 2 a 100 caracteres |

Las reglas simples viven en los DTOs (DataAnnotations). Las que necesitan la base de datos (que exista el responsable, que no tenga tareas al borrarlo) viven en los servicios.

## Decisiones que tomé

1. **Arquitectura en cuatro capas, sencilla.** Domain, Application, Infrastructure y Web, sin repositorios ni patrones extra. Los servicios usan EF Core a través de `IAppDbContext`, para que la lógica no dependa de la infraestructura.
2. **SQL Server con migraciones y datos de ejemplo.** Una migración inicial que ya incluye el *seed* (3 responsables y 12 tareas).
3. **El estado se guarda y se envía como texto.** Es legible en la base de datos y en el JSON, y no depende del orden del enum.
4. **`FechaLimite` como `DateOnly`.** Solo importa el día. La fecha pasada se rechaza únicamente al crear, para poder marcar como hecha una tarea ya vencida.
5. **Borrar un responsable con tareas está bloqueado (`409`).** Se protege en el servicio (mensaje claro) y en la base de datos (`DeleteBehavior.Restrict`). Descarté el borrado en cascada y dejar tareas sin responsable.
6. **Errores de negocio con excepciones propias** (`NotFound`, `Conflict`, `BusinessValidation`) y un **manejador global** que las traduce a `404`, `409` y `400` con `ProblemDetails`. Descarté el patrón *Result* por ser más código para el mismo resultado.
7. **Responsable inexistente dentro del cuerpo = `400`, no `404`.** El recurso consultado (`/api/tareas`) existe; lo inválido es el dato enviado.
8. **JWT con un único usuario definido en la configuración.** Suficiente para proteger la API sin añadir tablas, registro ni migraciones. La clave y la contraseña van en *user-secrets* y la aplicación valida la configuración al arrancar.
9. **La web consume la API por HTTP y se autentica sola** con un token de servicio que genera internamente. Sin pantalla de login. **Compromiso asumido:** la API directa está protegida, pero la interfaz web no pide credenciales (ver [Qué haría con más tiempo](#qué-haría-con-más-tiempo)).
10. **Tests en dos niveles.** Unitarios de servicios con SQLite en memoria (no el proveedor `InMemory`, que no impone claves foráneas ni se comporta como una base relacional) e integración con `WebApplicationFactory`, haciendo **login real** para cubrir el flujo JWT completo.
11. **Paginación con tope de 50** por página, para que nadie pida miles de registros de golpe. La web muestra 9 tarjetas por página para encajar en la cuadrícula de 3 columnas.
12. **Tras crear o editar, la web vuelve al listado** con un aviso de éxito.
13. **Responsables en una sola página**: alta rápida arriba, tarjetas debajo y ventana modal de confirmación para eliminar.
14. **Estilo moderno sin dependencias de compilación**: Bootstrap por CDN, CSS propio y modo oscuro.
15. **Historial de commits pequeños y con significado** (*Conventional Commits*), uno por paso.

## Qué haría con más tiempo

### Limitaciones conocidas

Las detecté en una revisión final y **decidí no corregirlas ahora**:

- **Los espacios se validan antes de recortarse.** Un título como `"   a   "` pasa la validación (mínimo 3 caracteres) y se guarda como `"a"`. Lo mismo ocurre con el nombre de un responsable. Solución: recortar antes de validar.
- **Una condición de carrera daría `500` en vez de `409`.** Si se asigna una tarea a un responsable justo mientras otra petición lo borra, la base de datos rechaza el borrado (`DbUpdateException`). Habría que traducir esa excepción a `409`.
- **`ProblemDetails` no es uniforme en los errores del propio framework.** Una ruta inexistente (`404`), un método no permitido (`405`), un tipo de contenido no soportado (`415`) o un `401` por falta de token devuelven el código correcto, pero sin cuerpo. Se arregla con `UseStatusCodePages` + `ProblemDetails`.
- **Se permiten responsables con el mismo nombre.** Habría que decidir si debe ser un `409`.
- **Sin control de concurrencia al editar.** Si dos personas editan la misma tarea, gana la última en guardar. Solución: `rowversion` y `409`/`412`.
- **Un error inesperado en una página Razor** se muestra como JSON de `ProblemDetails` en lugar de una página de error amigable.

### Seguridad

- **Pantalla de login en la web**: que el usuario inicie sesión, la web pida el token a la API y lo guarde en la sesión del servidor, de modo que la interfaz quede tan protegida como la API.
- **Usuarios en base de datos** con contraseñas con hash (por ejemplo ASP.NET Core Identity o `PasswordHasher`), roles y *refresh tokens*, en lugar del usuario único de la configuración.
- Límite de intentos en el login (*rate limiting*).

### Calidad y operación

- Tests de las páginas Razor y del `ApiClient`, y un test del camino del `500`.
- Tests contra un SQL Server real (por ejemplo con Testcontainers): SQLite es cómodo, pero no es idéntico.
- Integración continua con GitHub Actions (compilar y ejecutar los tests en cada *push*).
- `Dockerfile` y `docker-compose` con SQL Server, para arrancar todo con un solo comando.
- Documentación interactiva de la API (OpenAPI + Scalar) y *health checks*.
- Logging estructurado.

### Funcionalidad

- Editar responsables y reasignar sus tareas al borrarlos, en lugar de bloquear el borrado.
- Búsqueda por texto y ordenación configurable en el listado.
- Resumen de tareas por estado y avisos de tareas próximas a vencer.

## Cómo usé la IA

Este proyecto lo hice **trabajando con Claude (Anthropic) como asistente**, no dejando que lo generara todo.

**El criterio que seguí: la IA es una ayudante, no una sustituta.** Yo definí los requisitos, el alcance y las decisiones de diseño, y elegí qué aceptar, qué descartar y cuándo parar. Claude propuso alternativas, explicó compromisos y escribió borradores de código y de tests que yo revisé. Cuando la IA y yo no coincidimos, prevaleció mi decisión.

| | Yo | Claude |
|---|---|---|
| Requisitos y alcance | Los definí (incluidos los extras: JWT, estilos modernos, gestión de responsables) | Los desglosó en pasos |
| Diseño | Decidí en cada bifurcación | Presentó 2 o 3 opciones con ventajas, inconvenientes y una recomendación |
| Código y tests | Revisé, ejecuté y hice los commits | Escribió los borradores de cada paso |
| Calidad | Pedí una revisión crítica de lo hecho | Señaló huecos propios y reconoció lo que no podía comprobar |

### Método de trabajo

- **Paso a paso, con un commit por paso.** Pedí avanzar poco a poco y que cada paso terminara con su mensaje de commit. El historial de git refleja ese ritmo:

  1. `chore: estructura inicial de la solución TaskManager (.NET 10)`
  2. `feat(domain): añade entidades Tarea y Responsable y enum EstadoTarea`
  3. `feat(infra): configura EF Core con SQL Server, AppDbContext y mapeo de entidades`
  4. `feat(infra): añade migración inicial y datos de ejemplo (3 responsables, 12 tareas)`
  5. `feat(application): añade DTOs con validaciones, filtros y resultado paginado`
  6. `feat(application): añade servicios de tareas y responsables con excepciones de negocio`
  7. `feat(web): añade controllers de tareas y responsables y manejador global de errores (ProblemDetails)`
  8. `feat(auth): añade autenticación JWT con login y protege los endpoints de la API`
  9. `test: añade tests unitarios de servicios y validación de DTOs con SQLite en memoria`
  10. `test: añade tests de integración de la API con WebApplicationFactory, SQLite en memoria y login JWT real`
  11. `feat(web): añade cliente HTTP de la API, layout moderno con modo oscuro y listado de tareas con filtros y paginación`
  12. `feat(web): añade páginas de detalle, creación, edición y eliminación de tareas`
  13. `feat(web): añade gestión de responsables en una sola página con alta rápida y confirmación modal de borrado`
  14. `docs: añade README`

- **Cada paso terminaba con una decisión mía.** Antes de pasar al siguiente, Claude planteaba una pregunta con opciones y yo respondía.
- **Comprobación.** Claude no ejecuta código en este entorno, y lo dijo expresamente cuando le pregunté por el estado de los tests. Por eso las comprobaciones (compilar, ejecutar los tests, probar a mano cada pantalla con las tablas de casos de cada paso) las hago yo.

### Qué le pedí

- **Punto de partida:** una aplicación de gestión de tareas con los requisitos del enunciado (CRUD, filtros y paginación, validación y códigos HTTP, base de datos con migraciones, tests), más crear y eliminar responsables, en .NET 10, **paso a paso** y con una arquitectura **simple pero funcional**.
- **Después fui añadiendo peticiones propias:** un commit por paso hacia GitHub, JWT "cuando sea necesario", páginas Razor con estilos modernos y este README con especial énfasis en el uso de la IA.
- **Una revisión crítica al final:** pregunté si las pruebas, las validaciones y el manejo de errores con códigos HTTP estaban realmente hechos.

### Decisiones: qué recomendó Claude y qué elegí yo

| # | Decisión | Recomendación de Claude | Mi elección |
|---|---|---|---|
| 1 | Qué hacer al borrar un responsable con tareas | Bloquear con `409` | Bloquear con `409` |
| 2 | Cómo avisan los servicios de errores de negocio | Excepciones + manejador global | Excepciones + manejador global |
| 3 | Estado en el JSON | Texto | Texto |
| 4 | Origen de los usuarios del JWT | Usuario único en la configuración | Usuario único en la configuración |
| 5 | Autenticación en los tests de integración | Login real | Login real |
| 6 | Orden de trabajo | Tests de la API antes que la web | Tests antes que la web |
| 7 | Cómo obtiene el token la web | Pantalla de login en la web | **Credenciales de servicio, sin login** (contra la recomendación) |
| 8 | Adónde vuelve la web tras crear o editar | Al listado | Al listado |
| 9 | Pantalla de responsables | Una sola página con modal | Una sola página con modal |
| 10 | Documentar la API con OpenAPI y Scalar | Añadirlo antes del README | **Descartado**, pasé directo al README (contra la recomendación) |
| 11 | Corregir ya dos huecos detectados | Corregirlos ahora | **Los dejé documentados** y seguí (contra la recomendación) |

En la **7** Claude me avisó del inconveniente (la API queda protegida, pero la interfaz no pide credenciales). Asumí ese compromiso y lo dejé documentado como mejora futura.

### Qué acepté

En el chat acepté sin pedir cambios las propuestas de Claude para: la estructura de la solución, las entidades y su mapeo, el *seed*, los DTOs y sus validaciones, los servicios, los controllers, el manejador global de errores, el JWT, los tests (unitarios e integración) y las páginas Razor con sus estilos.

### Qué corregí o cuestioné

- **Cambié el rumbo en tres puntos** (decisiones 7, 10 y 11 de la tabla): ahí **la decisión fue mía y no la que recomendaba la IA**.
- **Amplié el alcance por mi cuenta.** Los requisitos de JWT, estilos modernos, commits por paso y el énfasis de este README los pedí yo; no salieron de la IA.
- **Pedí una auditoría crítica** en lugar de dar el trabajo por terminado. Claude no se limitó a contestar que sí: dijo que no había ejecutado los tests y señaló cinco huecos reales, que están en [Limitaciones conocidas](#limitaciones-conocidas).
- **Fijé el límite del alcance:** decidí no corregir esos huecos ahora y dejarlos documentados.

### Qué verifiqué con más cuidado

Estos son los puntos donde el código asistido es más fácil que falle y donde conviene no fiarse a ciegas:

- Los tests de integración sustituyen SQL Server por SQLite en memoria. En EF Core 9 y posteriores hay que retirar también la configuración registrada del proveedor original, no solo `DbContextOptions`; ese detalle depende de la versión de EF Core.
- Los tests pasan sobre SQLite, que no es idéntico a SQL Server (por ejemplo, en mayúsculas y minúsculas o en tipos de fecha).
- El código y las plantillas de .NET 10 son recientes: las versiones de paquetes y los detalles de las plantillas conviene confirmarlos con la documentación oficial.
