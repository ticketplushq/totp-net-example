using OtpNet;

public class TotpValidator
{
    private readonly TimeZoneInfo _timeZone;
    
    public TotpValidator(TimeZoneInfo? timeZone = null)
    {
        _timeZone = timeZone ?? TimeZoneInfo.Utc;
    }
    
    public bool ValidateTotp(string secret, string pin, string encoding = "ASCII")
    {
        // 1. Convertir el secreto según el encoding especificado
        byte[] secretKey = encoding.ToUpper() switch
        {
            "HEX" => Convert.FromHexString(secret),
            "BASE32" => Base32Encoding.ToBytes(secret),
            "ASCII" => System.Text.Encoding.ASCII.GetBytes(secret),
            "UTF8" => System.Text.Encoding.UTF8.GetBytes(secret),
            _ => throw new ArgumentException($"Encoding '{encoding}' no soportado. Use: HEX, BASE32, ASCII, UTF8")
        };
        
        // 2. Crear instancia TOTP
        var totp = new Totp(
            secretKey: secretKey,
            step: 30,
            mode: OtpHashMode.Sha1,
            totpSize: 6
        );
        
        // 3. Validar
        bool isValid = totp.VerifyTotp(
            totp: pin,
            out long timeStepMatched,
            window: VerificationWindow.RfcSpecifiedNetworkDelay
        );
        
        return isValid;
    }

    public void DiagnosticInfo(string secret, string pin, string encoding = "ASCII")
    {
        try
        {
            Console.WriteLine($"[DEBUG] ═══ INFORMACIÓN DE ENCODING ═══");
            Console.WriteLine($"[DEBUG] Secreto original: {secret}");
            Console.WriteLine($"[DEBUG] Encoding utilizado: {encoding.ToUpper()}");
            Console.WriteLine($"[DEBUG] Longitud secreto: {secret.Length} caracteres");
            
            // Convertir según el encoding actual
            byte[] secretKey = encoding.ToUpper() switch
            {
                "HEX" => Convert.FromHexString(secret),
                "BASE32" => Base32Encoding.ToBytes(secret),
                "ASCII" => System.Text.Encoding.ASCII.GetBytes(secret),
                "UTF8" => System.Text.Encoding.UTF8.GetBytes(secret),
                _ => throw new ArgumentException($"Encoding '{encoding}' no soportado")
            };
            
            Console.WriteLine($"[DEBUG] Secreto como bytes: {Convert.ToHexString(secretKey)}");
            Console.WriteLine($"[DEBUG] Bytes length: {secretKey.Length} bytes");
            Console.WriteLine();
            
            // Crear TOTP y validar
            var totp = new Totp(secretKey, step: 30, mode: OtpHashMode.Sha1, totpSize: 6);
            string currentCode = totp.ComputeTotp();
            long currentTimeStep = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / 30;
            
            Console.WriteLine($"[DEBUG] ═══ VALIDACIÓN ACTUAL ═══");
            Console.WriteLine($"[DEBUG] PIN ingresado: {pin}");
            Console.WriteLine($"[DEBUG] Código actual del sistema: {currentCode}");
            Console.WriteLine($"[DEBUG] Tiempo Unix actual: {DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");
            Console.WriteLine($"[DEBUG] Time step actual: {currentTimeStep}");
            Console.WriteLine($"[DEBUG] ¿Coincide exacto?: {currentCode == pin}");
            Console.WriteLine();
            
            // Verificar con ventanas de tiempo
            bool isValidCurrent = totp.VerifyTotp(pin, out long timeStepMatched);
            Console.WriteLine($"[DEBUG] Validación sin ventana: {isValidCurrent}, TimeStep matched: {timeStepMatched}");
            
            bool isValidWide = totp.VerifyTotp(pin, out long timeStepMatchedWide, VerificationWindow.RfcSpecifiedNetworkDelay);
            Console.WriteLine($"[DEBUG] Validación con ventana RFC: {isValidWide}, TimeStep matched: {timeStepMatchedWide}");
            Console.WriteLine();
            
            // Probar con otros encodings si es posible
            Console.WriteLine($"[DEBUG] ═══ PRUEBA CON OTROS ENCODINGS ═══");
            TestOtherEncodings(secret, pin, encoding);
            
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DEBUG] Error en diagnóstico: {ex.Message}");
        }
    }
    
    private void TestOtherEncodings(string secret, string pin, string currentEncoding)
    {
        string[] encodings = { "ASCII", "HEX", "BASE32", "UTF8" };
        
        foreach (string encoding in encodings)
        {
            if (encoding == currentEncoding.ToUpper()) continue;
            
            try
            {
                // Intentar convertir con este encoding
                byte[] testSecretKey = encoding switch
                {
                    "HEX" => secret.Length % 2 == 0 ? Convert.FromHexString(secret) : null,
                    "BASE32" => Base32Encoding.ToBytes(secret),
                    "ASCII" => System.Text.Encoding.ASCII.GetBytes(secret),
                    "UTF8" => System.Text.Encoding.UTF8.GetBytes(secret),
                    _ => null
                };
                
                if (testSecretKey == null)
                {
                    Console.WriteLine($"[DEBUG] {encoding}: FORMATO INVÁLIDO");
                    continue;
                }
                
                var testTotp = new Totp(testSecretKey, step: 30, mode: OtpHashMode.Sha1, totpSize: 6);
                string testCode = testTotp.ComputeTotp();
                bool isValid = testCode == pin;
                
                Console.WriteLine($"[DEBUG] {encoding}: {testCode} {(isValid ? "← MATCH!" : "")} ({testSecretKey.Length} bytes)");
                
            }
            catch
            {
                Console.WriteLine($"[DEBUG] {encoding}: ERROR - Formato incompatible");
            }
        }
    }
    
    // Helper para .NET Framework
    private byte[] HexStringToByteArray(string hex)
    {
        return Enumerable.Range(0, hex.Length)
                         .Where(x => x % 2 == 0)
                         .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                         .ToArray();
    }
}