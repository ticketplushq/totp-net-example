Console.WriteLine("=== Validador TOTP ===");
Console.WriteLine();

// Configurar zona horaria
Console.WriteLine("Seleccione la zona horaria:");
Console.WriteLine("1. UTC (por defecto)");
Console.WriteLine("2. Buenos Aires (America/Argentina/Buenos_Aires)");
Console.WriteLine("3. Local del sistema");
Console.Write("Ingrese su opción (1-3): ");

string? timezoneOption = Console.ReadLine();
TimeZoneInfo selectedTimeZone = TimeZoneInfo.Utc;

switch (timezoneOption)
{
    case "2":
        try
        {
            selectedTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Argentina/Buenos_Aires");
            Console.WriteLine("✓ Zona horaria configurada: Buenos Aires");
        }
        catch
        {
            Console.WriteLine("⚠ No se pudo configurar Buenos Aires, usando UTC");
        }
        break;
    case "3":
        selectedTimeZone = TimeZoneInfo.Local;
        Console.WriteLine($"✓ Zona horaria configurada: {selectedTimeZone.DisplayName}");
        break;
    default:
        Console.WriteLine("✓ Zona horaria configurada: UTC");
        break;
}

var validator = new TotpValidator(selectedTimeZone);
Console.WriteLine();

while (true)
{
    Console.Write("Ingrese el secreto hexadecimal (o 'exit' para salir): ");
    string? hexSecret = Console.ReadLine();
    
    if (string.IsNullOrEmpty(hexSecret) || hexSecret.ToLower() == "exit")
    {
        break;
    }
    
    Console.Write("Ingrese el PIN de 6 dígitos: ");
    string? userPin = Console.ReadLine();
    
    if (string.IsNullOrEmpty(userPin))
    {
        Console.WriteLine("PIN no válido. Intente de nuevo.");
        Console.WriteLine();
        continue;
    }
    
    try
    {
        // Usar encoding ASCII por defecto (como está configurado en ValidateTotp)
        string encoding = "ASCII";
        bool isValid = validator.ValidateTotp(hexSecret, userPin, encoding);
        
        if (isValid)
        {
            Console.WriteLine("✓ PIN VÁLIDO - Autenticación exitosa");
        }
        else
        {
            Console.WriteLine("✗ PIN INVÁLIDO - Autenticación fallida");
            Console.WriteLine();
            Console.WriteLine("--- INFORMACIÓN DE DIAGNÓSTICO ---");
            validator.DiagnosticInfo(hexSecret, userPin, encoding);
            Console.WriteLine("--- FIN DIAGNÓSTICO ---");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }
    
    Console.WriteLine();
}
