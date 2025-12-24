using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace Saver
{
    public class GoogleDriveService
    {
        private readonly DriveService _driveService;

        public GoogleDriveService()
        {
            // Шукаємо credentials.json в output папці проекту
            var credsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "credentials.json");

            if (!File.Exists(credsPath))
            {
                // Якщо не знайдено, шукаємо в корені проекту під час розробки
                credsPath = "credentials.json";
            }

            if (!File.Exists(credsPath))
            {
                throw new FileNotFoundException($"Google API credentials file not found. Make sure credentials.json is in the project with 'Copy to Output Directory = Always'");
            }

            UserCredential credential;

            using (var stream = new FileStream(credsPath, FileMode.Open, FileAccess.Read))
            {
                string tokenPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "token.json");
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    new[] { DriveService.Scope.Drive },
                    "user",
                    CancellationToken.None,
                    new FileDataStore(tokenPath, true)).Result;
            }

            _driveService = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "MauiApp24",
            });
        }

        public async Task UploadAsync(string fileName, string content, string mimeType)
        {
            try
            {
                if (string.IsNullOrEmpty(content))
                {
                    throw new ArgumentException("Контент для завантаження порожній");
                }

                // Перевірка кодування
                byte[] contentBytes = Encoding.UTF8.GetBytes(content);
                System.Diagnostics.Debug.WriteLine($"📊 Uploading {fileName}: {contentBytes.Length} bytes, UTF-8 BOM: {(contentBytes.Length >= 3 && contentBytes[0] == 0xEF && contentBytes[1] == 0xBB && contentBytes[2] == 0xBF)}");

                var fileMetadata = new Google.Apis.Drive.v3.Data.File()
                {
                    Name = fileName,
                    Description = $"Created {DateTime.Now:yyyy-MM-dd HH:mm:ss}"
                };

                using var stream = new MemoryStream(contentBytes);
                var request = _driveService.Files.Create(fileMetadata, stream, mimeType);
                request.Fields = "id, name, size, mimeType";

                var result = await request.UploadAsync();

                if (result.Status == Google.Apis.Upload.UploadStatus.Completed)
                {
                    var file = request.ResponseBody;
                    System.Diagnostics.Debug.WriteLine($"✅ Файл успішно завантажено: {file.Name} (ID: {file.Id}, Size: {file.Size} bytes)");
                }
                else
                {
                    throw new Exception($"Upload failed: {result.Exception?.Message}");
                }
            }
            catch (Google.GoogleApiException ex) when (ex.Error.Code == 401)
            {
                throw new Exception("Помилка авторизації Google Drive");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"💥 Upload Error: {ex}");
                throw;
            }
        }

        public async Task<string?> GetFileIdByNameAsync(string fileName)
        {
            var listRequest = _driveService.Files.List();
            listRequest.Q = $"name = '{fileName}' and trashed = false";
            listRequest.Fields = "files(id, name)";

            var files = await listRequest.ExecuteAsync();
            var file = files.Files.FirstOrDefault();

            return file?.Id;
        }

        public async Task<Stream> DownloadFileAsync(string fileId)
        {
            var request = _driveService.Files.Get(fileId);
            var stream = new MemoryStream();

            await request.DownloadAsync(stream);
            stream.Position = 0;

            return stream;
        }
    }
}