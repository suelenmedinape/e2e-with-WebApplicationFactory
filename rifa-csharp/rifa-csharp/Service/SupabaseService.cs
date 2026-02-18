using Supabase;
using Supabase.Core;
using System.Text.RegularExpressions;
using FileOptions = Supabase.Storage.FileOptions;

namespace rifa_csharp.Service;

public class SupabaseService
{
    private readonly Client supabase;
    private readonly string bucketName = "storage-iff-road";
    private readonly string folderName = "gallery";
    private readonly string supabaseUrl;
    private readonly string supabaseKey;

    public SupabaseService(IConfiguration configuration)
    {
        supabaseUrl = configuration["Supabase:Url"];
        supabaseKey = configuration["Supabase:Key"];

        if (string.IsNullOrEmpty(supabaseUrl) || string.IsNullOrEmpty(supabaseKey))
            throw new InvalidOperationException("Supabase URL e Key são obrigatórios");

        var options = new SupabaseOptions
        {
            AutoConnectRealtime = false
        };

        supabase = new Client(supabaseUrl, supabaseKey, options);
    }

    public async Task<string> UploadImageAsync(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
                throw new Exception("Arquivo vazio ou inválido");

            // Validações
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var fileExtension = Path.GetExtension(file.FileName).ToLower();
            
            if (!allowedExtensions.Contains(fileExtension))
                throw new Exception($"Tipo de arquivo não suportado: {fileExtension}");

            if (file.Length > 5 * 1024 * 1024) // 5MB
                throw new Exception("Arquivo muito grande. Máximo 5MB");

            Console.WriteLine($"[UPLOAD] Iniciando upload de {file.FileName}");

            // IMPORTANTE: Sanitize o nome do arquivo
            var sanitizedFileName = SanitizeFileName(file.FileName);
            var fileName = $"{Guid.NewGuid()}_{sanitizedFileName}";
            var filePath = $"{folderName}/{fileName}";

            Console.WriteLine($"[UPLOAD] Nome original: {file.FileName}");
            Console.WriteLine($"[UPLOAD] Nome sanitizado: {fileName}");

            // Converte para byte[]
            byte[] fileBytes;
            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                fileBytes = memoryStream.ToArray();
            }

            Console.WriteLine($"[UPLOAD] Arquivo convertido para bytes: {fileBytes.Length} bytes");

            // Upload usando Supabase SDK
            var response = await supabase.Storage
                .From(bucketName)
                .Upload(fileBytes, filePath, new FileOptions 
                { 
                    Upsert = false,
                    ContentType = file.ContentType
                });

            if (string.IsNullOrEmpty(response))
                throw new Exception("Falha no upload - resposta vazia do servidor");

            // Gera URL pública
            var publicUrl = supabase.Storage
                .From(bucketName)
                .GetPublicUrl(filePath);

            Console.WriteLine($"[UPLOAD] Sucesso! URL: {publicUrl}");
            return publicUrl;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERRO] Erro no upload: {ex.Message}");
            Console.WriteLine($"[ERRO] Stack trace: {ex.StackTrace}");
            throw new Exception($"Erro ao fazer upload: {ex.Message}", ex);
        }
    }
    
    public async Task<bool> DeleteImageAsync(string imageUrl)
    {
        try
        {
            if (string.IsNullOrEmpty(imageUrl))
                throw new Exception("URL da imagem vazia");

            var filePath = ExtractFilePathFromUrl(imageUrl);
            Console.WriteLine($"[DELETE] Deletando: {filePath}");

            await supabase.Storage
                .From(bucketName)
                .Remove(new List<string> { filePath });

            Console.WriteLine($"[DELETE] Sucesso!");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERRO] Erro ao deletar: {ex.Message}");
            throw new Exception($"Erro ao deletar imagem: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Remove caracteres especiais, acentuação e espaços do nome do arquivo
    /// Supabase Storage não aceita: espaços, acentos, parênteses, etc.
    /// </summary>
    private string SanitizeFileName(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
            return "file";

        // Remove acentuação
        var normalizedFileName = RemoveDiacritics(fileName);

        // Substitui espaços por underscore
        normalizedFileName = normalizedFileName.Replace(" ", "_");

        // Remove caracteres especiais, mantendo apenas letras, números, hífens, underscores e ponto
        normalizedFileName = Regex.Replace(normalizedFileName, @"[^a-zA-Z0-9._-]", "");

        // Remove múltiplos underscores consecutivos
        normalizedFileName = Regex.Replace(normalizedFileName, @"_{2,}", "_");

        // Remove múltiplos hífens consecutivos
        normalizedFileName = Regex.Replace(normalizedFileName, @"-{2,}", "-");

        // Remove múltiplos pontos consecutivos
        normalizedFileName = Regex.Replace(normalizedFileName, @"\.{2,}", ".");

        // Limita o tamanho total do nome (antes da extensão)
        var nameParts = normalizedFileName.Split('.');
        var baseName = nameParts[0];
        var extension = nameParts.Length > 1 ? "." + nameParts[nameParts.Length - 1] : "";

        if (baseName.Length > 100)
            baseName = baseName.Substring(0, 100);

        normalizedFileName = baseName + extension;

        // Se ficar vazio, retorna um padrão
        if (string.IsNullOrEmpty(normalizedFileName) || normalizedFileName == ".")
            return "file";

        return normalizedFileName;
    }

    /// <summary>
    /// Remove acentuação de caracteres (á, é, í, ó, ú, ã, õ, ç, etc.)
    /// Compatível com .NET 9.0
    /// </summary>
    private string RemoveDiacritics(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        try
        {
            // Usa decomposição Unicode padrão sem especificar NormalizationForm
            var normalizedString = text.Normalize();
            var stringBuilder = new System.Text.StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AVISO] Erro ao remover diacríticos: {ex.Message}");
            // Fallback: retorna o texto original se houver erro
            return text;
        }
    }

    private string ExtractFilePathFromUrl(string imageUrl)
    {
        var uri = new Uri(imageUrl);
        var path = uri.LocalPath;
        
        var bucketIndex = path.IndexOf($"/{bucketName}/");
        if (bucketIndex == -1)
            throw new Exception($"Formato de URL inválido. Esperado bucket '{bucketName}' na URL");
        
        return path.Substring(bucketIndex + bucketName.Length + 2);
    }
}