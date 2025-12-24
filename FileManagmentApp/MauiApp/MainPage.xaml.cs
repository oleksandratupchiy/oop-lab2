using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LogLibrary;
using MauiApp24.Parsers;
using Saver;

namespace MauiApp24
{
    public partial class MainPage : ContentPage
    {
        private List<Software> _allSoftware = new();
        private GoogleDriveSaver _driveSaver = new GoogleDriveSaver();
        private GoogleDriveService _driveService = new GoogleDriveService();

        public MainPage()
        {
            InitializeComponent();
        }

        private async void OnLoadClicked(object sender, EventArgs e)
        {
            try
            {
                Stream stream = null;
                string sourceName = "";

                if (RbLocal.IsChecked)
                {
                    stream = await FileSystem.OpenAppPackageFileAsync("software_data.xml");
                    sourceName = "Локальний файл";
                }
                else
                {
                    string fileName = DriveFileNameEntry.Text;
                    if (string.IsNullOrEmpty(fileName))
                    {
                        await DisplayAlert("Помилка", "Введіть назву файлу XML на Google Drive", "ОК");
                        return;
                    }

                    string fileId = await _driveService.GetFileIdByNameAsync(fileName);
                    if (fileId == null)
                    {
                        await DisplayAlert("Помилка", "Файл не знайдено на Google Drive", "ОК");
                        return;
                    }

                    stream = await _driveService.DownloadFileAsync(fileId);
                    sourceName = $"Google Drive ({fileName})";

                    // ДІАГНОСТИКА: створюємо КОПІЮ потоку для перевірки
                    stream.Position = 0;
                    using var memoryStream = new MemoryStream();
                    await stream.CopyToAsync(memoryStream);
                    memoryStream.Position = 0;

                    using var reader = new StreamReader(memoryStream);
                    string xmlContent = await reader.ReadToEndAsync();
                    System.Diagnostics.Debug.WriteLine($"=== ВМІСТ XML ФАЙЛУ ===");
                    System.Diagnostics.Debug.WriteLine(xmlContent.Substring(0, Math.Min(500, xmlContent.Length)));

                    // Повертаємо оригінальний потік для парсингу
                    stream.Position = 0;
                }

                IParsingStrategy strategy;
                string strategyName = "";

                switch (StrategyPicker.SelectedIndex)
                {
                    case 1:
                        strategy = new SAXParsingStrategy();
                        strategyName = "SAX API";
                        break;
                    case 2:
                        strategy = new DOMParsingStrategy();
                        strategyName = "DOM API";
                        break;
                    default:
                        strategy = new LINQParsingStrategy();
                        strategyName = "LINQ to XML";
                        break;
                }

                // КОПІЮЄМО потік перед парсингом
                using var streamCopy = new MemoryStream();
                await stream.CopyToAsync(streamCopy);
                streamCopy.Position = 0;
                stream.Dispose(); // Закриваємо оригінальний потік

                _allSoftware = strategy.Parse(streamCopy);

                // Перевірка результатів парсингу
                System.Diagnostics.Debug.WriteLine($"=== РЕЗУЛЬТАТ ПАРСИНГУ ===");
                System.Diagnostics.Debug.WriteLine($"Знайдено записів: {_allSoftware.Count}");
                if (_allSoftware.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"Перший запис:");
                    System.Diagnostics.Debug.WriteLine($"- Name: '{_allSoftware[0].Name}'");
                    System.Diagnostics.Debug.WriteLine($"- Version: '{_allSoftware[0].Version}'");
                    System.Diagnostics.Debug.WriteLine($"- FullName: '{_allSoftware[0].FullName}'");
                }

                // Оновлення UI
                SoftwareCollectionView.ItemsSource = null;
                SoftwareCollectionView.ItemsSource = _allSoftware;

                Logger.Instance.Log("Інформація", $"Завантажено з {sourceName}. Метод: {strategyName}. Записів: {_allSoftware.Count}");

                await DisplayAlert("Успіх", $"Завантажено {_allSoftware.Count} записів.", "ОК");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"💥 ПОМИЛКА: {ex.Message}");
                Logger.Instance.Log("Помилка", $"Збій завантаження: {ex.Message}");
                await DisplayAlert("Помилка", ex.Message, "ОК");
            }
        }

        private void OnSearchClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchEntry.Text)) return;

            var keyword = SearchEntry.Text.ToLower();
            var selectedAttribute = AttributePicker.SelectedIndex;

            List<Software> filtered;

            switch (selectedAttribute)
            {
                case 0:
                    filtered = _allSoftware.Where(s => s.Name.ToLower().Contains(keyword)).ToList();
                    break;
                case 1:
                    filtered = _allSoftware.Where(s => s.Author.ToLower().Contains(keyword)).ToList();
                    break;
                case 2:
                    filtered = _allSoftware.Where(s => s.Type.ToLower().Contains(keyword)).ToList();
                    break;
                case 3:
                    filtered = _allSoftware.Where(s => s.Annotation.ToLower().Contains(keyword)).ToList();
                    break;
                default:
                    filtered = _allSoftware;
                    break;
            }

            SoftwareCollectionView.ItemsSource = null;
            SoftwareCollectionView.ItemsSource = filtered;

            string attributeName = AttributePicker.SelectedItem?.ToString() ?? "Назва";
            Logger.Instance.Log("Фільтрація", $"Параметри: «{attributeName}» – {keyword}. Знайдено: {filtered.Count}");
        }

        private void OnClearClicked(object sender, EventArgs e)
        {
            SearchEntry.Text = "";
            AttributePicker.SelectedIndex = 0;
            SoftwareCollectionView.ItemsSource = null;
            SoftwareCollectionView.ItemsSource = _allSoftware;
        }

        private async void OnSaveHtmlClicked(object sender, EventArgs e)
        {
            var currentList = SoftwareCollectionView.ItemsSource as List<Software> ?? _allSoftware;
            if (currentList == null || !currentList.Any()) return;

            try
            {
                string xsltContent = null;

                bool useDriveXslt = await DisplayAlert("XSLT", "Завантажити шаблон XSLT з Google Drive?", "Так", "Ні (стандартний)");

                if (useDriveXslt)
                {
                    string xsltName = await DisplayPromptAsync("XSLT", "Введіть назву файлу .xsl на Drive:");
                    if (!string.IsNullOrEmpty(xsltName))
                    {
                        string fileId = await _driveService.GetFileIdByNameAsync(xsltName);
                        if (fileId != null)
                        {
                            using var stream = await _driveService.DownloadFileAsync(fileId);
                            using var reader = new StreamReader(stream);
                            xsltContent = await reader.ReadToEndAsync();
                        }
                        else
                        {
                            await DisplayAlert("Увага", "Файл XSLT не знайдено, буде використано стандартний.", "ОК");
                        }
                    }
                }

                var saver = new HtmlSaver(xsltContent);
                string content = saver.GenerateContent(currentList);
                string path = Path.Combine(FileSystem.CacheDirectory, "report.html");

                await File.WriteAllTextAsync(path, content);

                Logger.Instance.Log("Трансформація", $"Збережено у файл {path}");

                await DisplayAlert("Збережено", $"Шлях: {path}", "ОК");
            }
            catch (Exception ex)
            {
                Logger.Instance.Log("Помилка", $"Трансформація не вдалася: {ex.Message}");
                await DisplayAlert("Помилка", ex.Message, "ОК");
            }
        }

        private async void OnSaveDriveClicked(object sender, EventArgs e)
        {
            var currentList = SoftwareCollectionView.ItemsSource as List<Software> ?? _allSoftware;
            if (currentList == null || !currentList.Any())
            {
                await DisplayAlert("Помилка", "Немає даних для збереження", "ОК");
                return;
            }

            try
            {
                // Запитуємо формат
                string format = await DisplayActionSheet("Формат файлу", "Скасувати", null, "HTML", "XML");
                if (format == "Скасувати" || format == null) return;

                // Запитуємо XSLT для HTML (якщо потрібно)
                string xsltContent = null;
                if (format == "HTML")
                {
                    bool useCustomXslt = await DisplayAlert("XSLT", "Використати кастомний XSLT з Google Drive?", "Так", "Ні");
                    if (useCustomXslt)
                    {
                        string xsltName = await DisplayPromptAsync("XSLT", "Введіть назву файлу .xsl на Drive:");
                        if (!string.IsNullOrEmpty(xsltName))
                        {
                            string fileId = await _driveService.GetFileIdByNameAsync(xsltName);
                            if (fileId != null)
                            {
                                using var stream = await _driveService.DownloadFileAsync(fileId);
                                using var reader = new StreamReader(stream);
                                xsltContent = await reader.ReadToEndAsync();
                            }
                            else
                            {
                                await DisplayAlert("Увага", "Файл XSLT не знайдено, буде використано стандартний.", "ОК");
                            }
                        }
                    }
                }

                // Створюємо відповідний Saver
                ISaver saver;
                if (format == "HTML")
                {
                    saver = new HtmlSaver(xsltContent);  // Передаємо XSLT
                }
                else
                {
                    saver = new XmlSaver();
                }

                string fileContent = saver.GenerateContent(currentList);
                string extension = format == "XML" ? ".xml" : ".html";
                string fileName = $"software_report_{DateTime.Now:yyyyMMdd_HHmmss}{extension}";
                string mimeType = format == "XML" ? "application/xml" : "text/html";

                // Зберігаємо на Google Drive
                await _driveService.UploadAsync(fileName, fileContent, mimeType);

                Logger.Instance.Log("Збереження", $"Збережено {currentList.Count} записів у {format} на Google Drive");
                await DisplayAlert("Успіх", $"Файл {fileName} завантажено на Google Drive!", "ОК");
            }
            catch (Exception ex)
            {
                Logger.Instance.Log("Помилка", $"Збій збереження на Drive: {ex.Message}");
                await DisplayAlert("Помилка", ex.Message, "ОК");
            }
        }
        private async void OnExitClicked(object sender, EventArgs e)
        {
            if (await DisplayAlert("Вихід", "Чи дійсно ви хочете завершити роботу з програмою?", "Так", "Ні"))
            {
                Application.Current.Quit();
            }
        }
    }
}