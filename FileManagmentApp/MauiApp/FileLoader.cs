using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiApp24
{
    public static class FileLoader
    {
        public static async Task<string?> SelectFileAsync()
        {
            try
            {
                var pickOptions = new PickOptions
                {
                    PickerTitle = "Select an XML file",
                    FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.WinUI, new[] { ".xml" } }
                })
                };

                var result = await FilePicker.Default.PickAsync(pickOptions);
                return result?.FullPath;
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Oops", $"Error selecting file: {ex.Message}", "OK");
                return null;
            }
        }
    }

}
