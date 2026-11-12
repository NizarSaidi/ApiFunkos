# Pruebas automáticas de FuncoApi con Bruno

La colección prueba el inicio de sesión y los flujos CRUD de Funkos, categorías y proveedores. Crea registros temporales con nombres únicos y los elimina al final; ejecútala contra una base de datos local o de pruebas, nunca contra producción.

## Preparación

1. Por defecto, `appsettings.Development.json` usa almacenamiento en memoria para SQL Server y MongoDB; no es necesario iniciar esos servidores. Los datos se reinician al cerrar la API.
2. En PowerShell, configura la contraseña para que la API cree el administrador de desarrollo: `$env:DevelopmentSeed__AdminPassword = "UnaClaveLocalSegura123!"`. La cuenta seed es `admin@funco.local`. El endpoint público de registro crea usuarios con rol `User`.
3. Desde la carpeta `FuncoApi` que contiene `Bruno`, arranca la API con `dotnet run --project .\FuncoApi\FuncoApi.csproj`.
4. Copia `environments/Local.example.bru` como `environments/Local.bru` y configura `baseUrl`. En Bruno, añade `adminPassword` desde **Secrets** del entorno con el mismo valor de `DevelopmentSeed:AdminPassword`; Bruno guarda ese valor localmente cifrado. El archivo `Local.bru` está ignorado por Git.
5. Configura un transporte SMTP válido para la API. El inicio de sesión envía un correo y devuelve error si el envío falla.
6. Abre `Bruno/FuncoApi` en Bruno, selecciona el entorno `Local` y ejecuta la colección completa en orden.

La colección verifica códigos HTTP, campos de respuesta e IDs generados. La creación de Funkos también intenta notificar a los usuarios, pero un fallo de correo no cancela ese CRUD.

Para usar las bases reales durante el desarrollo, inicia SQL Server y MongoDB y establece `$env:Database__UseInMemory = "false"` antes de arrancar la API. También puedes cambiar `Database:UseInMemory` en `appsettings.Development.json`. En producción el valor predeterminado es `false`; las conexiones se toman de `ConnectionStrings:FuncoDb` y `MongoSettings`. Puedes sobrescribirlo con `Database__UseInMemory=true` si necesitas iniciar temporalmente sin servidores.

Para ejecutarla desde la terminal, instala Bruno CLI y, desde esta carpeta, ejecuta `bru run --env Local`. Los valores secretos configurados en Bruno Desktop no se exportan al CLI; para CI, inyéctalos desde el almacén de secretos del pipeline.
