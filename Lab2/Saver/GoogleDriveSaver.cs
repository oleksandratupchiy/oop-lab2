using MauiApp24.Parsers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Saver
{
    public class GoogleDriveSaver
    {
        private readonly GoogleDriveService _driveService;

        public GoogleDriveSaver()
        {
            _driveService = new GoogleDriveService();
        }

        public async Task SaveToGoogleDriveAsync(List<Software> softwareList, string selectedFormat)
        {
            // Використовуємо твою існуючу фабрику
            var saver = SaverFactory.GetSaver(selectedFormat);
            string fileContent = saver.GenerateContent(softwareList);

            // Генеруємо унікальне ім'я файлу
            string extension = selectedFormat == "XML" ? ".xml" : ".html";
            string fileName = $"software_report_{DateTime.Now:yyyyMMdd_HHmmss}{extension}";
            string mimeType = selectedFormat == "XML" ? "application/xml" : "text/html";

            // Завантажуємо на Google Drive
            await _driveService.UploadAsync(fileName, fileContent, mimeType);
        }
    }
}