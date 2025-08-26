# Validador TOTP en .NET

Este proyecto implementa un validador de TOTP (Time-based One-Time Password) en .NET que permite validar códigos de 6 dígitos usando secretos en múltiples formatos de encoding.

## Requisitos

- .NET 9.0 o superior
- Paquete NuGet: Otp.NET

## Estructura del proyecto

```
totp-net-example/
├── README.md           # Este archivo
├── .gitignore          # Archivos ignorados por Git
└── TotpValidator/      # Proyecto .NET
    ├── Program.cs          # Aplicación de consola principal
    ├── TotpValidator.cs    # Clase validadora TOTP
    └── TotpValidator.csproj # Archivo de proyecto
```

## Instalación y ejecución

### 1. Clonar o descargar el proyecto

```bash
git clone https://github.com/ticketplushq/totp-net-example.git
cd totp-net-example
```

### 2. Navegar al directorio del proyecto

```bash
cd TotpValidator
```

### 3. Restaurar las dependencias

```bash
dotnet restore
```

### 4. Compilar el proyecto

```bash
dotnet build
```

### 5. Ejecutar la aplicación

```bash
dotnet run
```

## Uso de la aplicación

Al ejecutar la aplicación, primero debes seleccionar la zona horaria y luego ingresar el secreto y PIN:

```
=== Validador TOTP ===

Seleccione la zona horaria:
1. UTC (por defecto)
2. Buenos Aires (America/Argentina/Buenos_Aires)
3. Local del sistema
Ingrese su opción (1-3): [tu opción]

Ingrese el secreto hexadecimal (o 'exit' para salir): [tu secreto]
Ingrese el PIN de 6 dígitos: [tu código TOTP]
```

### Ejemplo de uso exitoso

```
=== Validador TOTP ===

Seleccione la zona horaria:
1. UTC (por defecto)
2. Buenos Aires (America/Argentina/Buenos_Aires)
3. Local del sistema
Ingrese su opción (1-3): 1
✓ Zona horaria configurada: UTC

Ingrese el secreto hexadecimal (o 'exit' para salir): 700600dad2d0b8268b5329925e6105be
Ingrese el PIN de 6 dígitos: 898386
✓ PIN VÁLIDO - Autenticación exitosa

Ingrese el secreto hexadecimal (o 'exit' para salir): exit
```

### Ejemplo con diagnóstico automático mejorado

Cuando un PIN es inválido, la aplicación muestra información de diagnóstico detallada automáticamente:

```
Ingrese el PIN de 6 dígitos: 000000
✗ PIN INVÁLIDO - Autenticación fallida

--- INFORMACIÓN DE DIAGNÓSTICO ---
[DEBUG] ═══ INFORMACIÓN DE ENCODING ═══
[DEBUG] Secreto original: 700600dad2d0b8268b5329925e6105be
[DEBUG] Encoding utilizado: ASCII
[DEBUG] Longitud secreto: 32 caracteres
[DEBUG] Secreto como bytes: 37303036303064616432643062383236386235333239393235653631303562650
[DEBUG] Bytes length: 32 bytes

[DEBUG] ═══ VALIDACIÓN ACTUAL ═══
[DEBUG] PIN ingresado: 000000
[DEBUG] Código actual del sistema: 729672
[DEBUG] Tiempo Unix actual: 1756241365
[DEBUG] Time step actual: 58541410
[DEBUG] ¿Coincide exacto?: false
[DEBUG] Validación sin ventana: False, TimeStep matched: 0
[DEBUG] Validación con ventana RFC: False, TimeStep matched: 0

[DEBUG] ═══ PRUEBA CON OTROS ENCODINGS ═══
[DEBUG] HEX: 898386 ← MATCH! (16 bytes)
[DEBUG] BASE32: ERROR - Formato incompatible
[DEBUG] UTF8: 456789 (32 bytes)
--- FIN DIAGNÓSTICO ---
```

**En este ejemplo**, el diagnóstico detecta que el PIN `000000` no es válido con encoding ASCII, pero **sí es válido con encoding HEX**, mostrando `← MATCH!` y sugiriendo el encoding correcto.

## Configuración TOTP

La implementación utiliza los siguientes parámetros TOTP estándar:

- **Algoritmo**: SHA1
- **Período de tiempo**: 30 segundos
- **Longitud del código**: 6 dígitos
- **Ventana de verificación**: RFC Specified Network Delay (permite cierta tolerancia de tiempo)
- **Encoding por defecto**: ASCII (soporta HEX, BASE32, UTF8)

## Encodings soportados

El validador soporta múltiples formatos de encoding para el secreto:

### ASCII (Predeterminado)
```csharp
validator.ValidateTotp("MySecretKey123", pin);
validator.ValidateTotp("MySecretKey123", pin, "ASCII");
```

### HEX (Hexadecimal)
```csharp
validator.ValidateTotp("700600dad2d0b8268b5329925e6105be", pin, "HEX");
```

### BASE32
```csharp
validator.ValidateTotp("OADABWWS2C4CNC2TFGJF4YIFXY======", pin, "BASE32");
```

### UTF8
```csharp
validator.ValidateTotp("MiClaveSecreta", pin, "UTF8");
```

## Estructura del código

### TotpValidator.cs

Contiene la clase `TotpValidator` con:

- **`ValidateTotp(string secret, string pin, string encoding = "ASCII")`**: Método principal que valida un código TOTP
  - `secret`: El secreto en el formato especificado por encoding
  - `pin`: El código TOTP de 6 dígitos a validar
  - `encoding`: El formato del secreto (ASCII, HEX, BASE32, UTF8)
  
- **`DiagnosticInfo(string secret, string pin, string encoding = "ASCII")`**: Método de diagnóstico que se ejecuta automáticamente cuando falla la validación
  - Muestra información detallada sobre el encoding utilizado
  - Prueba automáticamente con otros encodings y detecta coincidencias
  - Incluye códigos TOTP actuales, time steps y validaciones con ventana
  - **Detecta el encoding correcto**: Marca con `← MATCH!` si encuentra coincidencia con otro encoding
  
- **`HexStringToByteArray(string hex)`**: Método auxiliar para compatibilidad con versiones anteriores de .NET Framework

### Program.cs

Aplicación de consola que:

1. **Configura la zona horaria**: Permite seleccionar UTC, Buenos Aires o sistema local
2. **Solicita el secreto**: En formato hexadecimal (se interpreta como ASCII por defecto)  
3. **Solicita el PIN**: Código TOTP de 6 dígitos
4. **Valida el código**: Usando la clase TotpValidator con encoding ASCII
5. **Muestra diagnóstico**: Información detallada automática si falla la validación
6. **Permite múltiples validaciones**: Hasta que el usuario escriba 'exit'

## Interoperabilidad

Este validador es compatible con otras implementaciones TOTP que usen los mismos estándares:

### Configuración estándar TOTP

```csharp
validator.ValidateTotp("700600dad2d0b8268b5329925e6105be", pin, "ASCII");
```

### Parámetros estándar

- **Algoritmo**: SHA1
- **Longitud**: 6 dígitos  
- **Período**: 30 segundos
- **Encoding**: Configurable (ASCII, HEX, BASE32, UTF8)

### Ejemplo funcionando

```
Generador TOTP → PIN: 898386
C# Validator   → PIN: 898386 ✓ VÁLIDO
```

## Notas técnicas

- **Secretos soportados**: ASCII (predeterminado), HEX, BASE32, UTF8
- **Longitud del PIN**: Exactamente 6 dígitos
- **Manejo de errores**: Automático con información de diagnóstico
- **Compatibilidad .NET**: 5+ con `Convert.FromHexString()`
- **Tolerancia de tiempo**: RFC Specified Network Delay
- **Zona horaria**: Configurable (UTC, Buenos Aires, local)

## Dependencias

- **Otp.NET**: Librería para generar y validar códigos TOTP/HOTP
  - Versión: 1.4.0
  - Instalación automática mediante `dotnet restore`