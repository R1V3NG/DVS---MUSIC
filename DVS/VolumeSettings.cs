using System;
using System.IO;
using System.Xml.Linq;
using System.Reflection;

public static class VolumeSettings
{
    // Используем readonly для динамического пути
    private static readonly string FilePath; //C:\Users\Привет\source\repos\DVS---MUSIC-Sergei\DVS\VolumeSettings.xml (то что я использую лежит в bin)

    // Статический конструктор для инициализации пути
    static VolumeSettings()
    {
        // Получаем путь к директории, где находится исполняемый файл
        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        // Получаем путь к родительскому каталогу
        var parentDirectory = Directory.GetParent(baseDirectory).FullName;
        // Путь к DVS
        FilePath = Path.Combine(parentDirectory, "volumeSettings.xml");
    }

    public static double LoadVolume()
    {
        if (File.Exists(FilePath))
        {
            var xml = XDocument.Load(FilePath);
            var volumeElement = xml.Root.Element("Volume");
            if (volumeElement != null)
            {
                // Заменяем точку на запятую, если это необходимо
                volumeElement.Value = volumeElement.Value.Replace(".", ",");
                if (double.TryParse(volumeElement.Value, out double volume))
                {
                    return volume;
                }
            }
        }
        return 0.5; // Значение по умолчанию, если файл не существует или чтение не удалось
    }

    public static void SaveVolume(double volume)
    {
        var xml = new XDocument(
            new XElement("Settings",
                new XElement("Volume", volume) // Сохраняем в формате с точкой
            )
        );
    
        if (File.Exists(FilePath))
        xml.Save(FilePath); // Сохраняем файл без проверки, так как мы его создаем
    }   
}