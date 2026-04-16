using System;
using System.Collections.Generic;
using System.IO;

[System.Serializable]
public class SubtitleEntry
{
    public float startTime;
    public float endTime;
    public string text;
}

public static class SRTParser
{
    public static List<SubtitleEntry> Parse(string filePath)
    {
        var entries = new List<SubtitleEntry>();
        string[] lines = File.ReadAllLines(filePath);

        int i = 0;
        while (i < lines.Length)
        {
            // Saltar líneas vacías y números de índice
            if (string.IsNullOrWhiteSpace(lines[i]) || int.TryParse(lines[i].Trim(), out _))
            {
                i++;
                continue;
            }

            // Leer timestamps: "00:00:01,000 --> 00:00:04,500"
            if (lines[i].Contains("-->"))
            {
                string[] parts = lines[i].Split(new string[] { " --> " }, StringSplitOptions.None);
                float start = ParseTime(parts[0].Trim());
                float end = ParseTime(parts[1].Trim());
                i++;

                // Leer líneas de texto hasta línea vacía
                string text = "";
                while (i < lines.Length && !string.IsNullOrWhiteSpace(lines[i]))
                {
                    text += lines[i] + "\n";
                    i++;
                }

                entries.Add(new SubtitleEntry
                {
                    startTime = start,
                    endTime = end,
                    text = text.Trim()
                });
            }
            else
            {
                i++;
            }
        }

        return entries;
    }

    // Convierte "00:01:23,456" a segundos (float)
    private static float ParseTime(string timeStr)
    {
        // Reemplaza coma por punto para el parse de milisegundos
        timeStr = timeStr.Replace(',', '.');
        string[] parts = timeStr.Split(':');

        float hours = float.Parse(parts[0]);
        float minutes = float.Parse(parts[1]);
        float seconds = float.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture);

        return hours * 3600f + minutes * 60f + seconds;
    }
}