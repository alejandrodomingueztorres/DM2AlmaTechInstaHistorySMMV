using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

public class SimplePDFReportGenerator : MonoBehaviour
{
    [Header("Archivo PDF")]
    [SerializeField] private string pdfFileName = "Reporte_Metricas_InstaHistory_v4.pdf";

    [Header("Logos PNG en Assets/StreamingAssets/Logos")]
    [SerializeField] private string projectLogoFileName = "logo_instahistory.png";
    [SerializeField] private string universityLogoFileName = "logo_uao.png";

    private const string TotalVisitorsKey = "Metrics_TotalVisitors";
    private const string TotalSessionDurationKey = "Metrics_TotalSessionDuration";
    private const string TotalInteractionsKey = "Metrics_TotalInteractions";
    private const string CompletedStagesKey = "Metrics_CompletedStages";

    public void GenerateMetricsReport()
    {
        string folderPath = Application.persistentDataPath;
        string filePath = Path.Combine(folderPath, pdfFileName);

        try
        {
            byte[] pdfBytes = BuildStyledPdf();
            File.WriteAllBytes(filePath, pdfBytes);
            UnityEngine.Debug.Log("PDF generado correctamente en: " + filePath);
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError("Error al generar el PDF: " + ex.Message);
        }
    }

    public void OpenReportFolder()
    {
        string folderPath = Application.persistentDataPath;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        Process.Start("explorer.exe", folderPath.Replace("/", "\\"));
#else
        Application.OpenURL(folderPath);
#endif
    }

    public void ResetMetrics()
    {
        PlayerPrefs.DeleteKey(TotalVisitorsKey);
        PlayerPrefs.DeleteKey(TotalSessionDurationKey);
        PlayerPrefs.DeleteKey(TotalInteractionsKey);
        PlayerPrefs.DeleteKey(CompletedStagesKey);
        PlayerPrefs.Save();

        UnityEngine.Debug.Log("Métricas reiniciadas correctamente.");
    }

    private byte[] BuildStyledPdf()
    {
        using MemoryStream ms = new MemoryStream();
        List<long> offsets = new List<long>();

        PdfRasterImage projectLogo = LoadPngAsJpgForPdf(projectLogoFileName);
        PdfRasterImage uaoLogo = LoadPngAsJpgForPdf(universityLogoFileName);

        bool hasProjectLogo = projectLogo != null;
        bool hasUaoLogo = uaoLogo != null;

        void WriteString(string value)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(value);
            ms.Write(bytes, 0, bytes.Length);
        }

        void WriteBytes(byte[] bytes)
        {
            ms.Write(bytes, 0, bytes.Length);
        }

        void WriteObject(int number, string content)
        {
            offsets.Add(ms.Position);
            WriteString(number + " 0 obj\n");
            WriteString(content);
            WriteString("\nendobj\n");
        }

        WriteString("%PDF-1.4\n");

        WriteObject(1, "<< /Type /Catalog /Pages 2 0 R >>");
        WriteObject(2, "<< /Type /Pages /Kids [3 0 R] /Count 1 >>");

        string xObjects = "";
        int nextImageObject = 7;

        if (hasProjectLogo) xObjects += "/ImProject " + nextImageObject++ + " 0 R ";
        if (hasUaoLogo) xObjects += "/ImUAO " + nextImageObject++ + " 0 R ";

        string pageObject =
            "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Contents 4 0 R " +
            "/Resources << /Font << /FRegular 5 0 R /FBold 6 0 R >> ";

        if (!string.IsNullOrEmpty(xObjects))
            pageObject += "/XObject << " + xObjects + ">> ";

        pageObject += ">> >>";

        WriteObject(3, pageObject);

        string content = BuildContentStream(hasProjectLogo, hasUaoLogo, projectLogo, uaoLogo);
        byte[] contentBytes = Encoding.ASCII.GetBytes(content);

        offsets.Add(ms.Position);
        WriteString("4 0 obj\n");
        WriteString("<< /Length " + contentBytes.Length + " >>\n");
        WriteString("stream\n");
        WriteBytes(contentBytes);
        WriteString("\nendstream\nendobj\n");

        WriteObject(5, "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>");
        WriteObject(6, "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold >>");

        int objectId = 7;

        if (hasProjectLogo)
            WriteImageObject(objectId++, projectLogo);

        if (hasUaoLogo)
            WriteImageObject(objectId++, uaoLogo);

        long xrefPos = ms.Position;

        WriteString("xref\n");
        WriteString("0 " + objectId + "\n");
        WriteString("0000000000 65535 f \n");

        foreach (long offset in offsets)
            WriteString(offset.ToString("D10", CultureInfo.InvariantCulture) + " 00000 n \n");

        WriteString("trailer\n");
        WriteString("<< /Size " + objectId + " /Root 1 0 R >>\n");
        WriteString("startxref\n");
        WriteString(xrefPos.ToString(CultureInfo.InvariantCulture) + "\n");
        WriteString("%%EOF");

        return ms.ToArray();

        void WriteImageObject(int number, PdfRasterImage img)
        {
            offsets.Add(ms.Position);
            WriteString(number + " 0 obj\n");
            WriteString("<< /Type /XObject /Subtype /Image ");
            WriteString("/Width " + img.Width + " ");
            WriteString("/Height " + img.Height + " ");
            WriteString("/ColorSpace /DeviceRGB ");
            WriteString("/BitsPerComponent 8 ");
            WriteString("/Filter /DCTDecode ");
            WriteString("/Length " + img.JpegBytes.Length + " >>\n");
            WriteString("stream\n");
            WriteBytes(img.JpegBytes);
            WriteString("\nendstream\nendobj\n");
        }
    }

    private string BuildContentStream(bool hasProjectLogo, bool hasUaoLogo, PdfRasterImage projectLogo, PdfRasterImage uaoLogo)
    {
        StringBuilder sb = new StringBuilder();

        MetricsSnapshot metrics = GetMetricsSnapshot();

        string visitors = metrics.TotalVisitors.ToString(CultureInfo.InvariantCulture);
        string totalDuration = metrics.TotalSessionDuration.ToString("F2", CultureInfo.InvariantCulture);
        string avgSession = metrics.AverageSessionDuration.ToString("F2", CultureInfo.InvariantCulture);
        string interactions = metrics.TotalInteractions.ToString(CultureInfo.InvariantCulture);
        string stages = metrics.CompletedStages.ToString(CultureInfo.InvariantCulture);
        string date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

        FillRect(sb, 0, 0, 595, 842, "#FFFFFF");
        StrokeRect(sb, 18, 18, 559, 806, "#6B4A32", 1.8f);

        FillRect(sb, 18, 720, 559, 104, "#055169");
        FillRect(sb, 18, 710, 559, 10, "#017FBD");
        FillRect(sb, 18, 704, 559, 6, "#D19525");

        if (hasProjectLogo)
        {
            FillRect(sb, 34, 740, 120, 50, "#FFFFFF");
            StrokeRect(sb, 34, 740, 120, 50, "#E5D3AD", 1f);

            RectFit fit = FitInsideBox(projectLogo.Width, projectLogo.Height, 40, 744, 108, 42);
            DrawImage(sb, "ImProject", fit.X, fit.Y, fit.Width, fit.Height);
        }

        DrawText(sb, "FBold", 22, 180, 780, "Reporte administrativo", "#FFFFFF");
        DrawText(sb, "FRegular", 10, 180, 760, "Metricas de interaccion y apropiacion cultural", "#E5D3AD");

        FillRect(sb, 48, 350, 499, 288, "#E5D3AD");
        StrokeRect(sb, 48, 350, 499, 288, "#6B4A32", 1.2f);

        DrawText(sb, "FBold", 16, 64, 612, "Resumen de metricas registradas", "#6B4A32");
        DrawLine(sb, 64, 602, 532, 602, "#4FB3BF", 1.2f);

        DrawMetricRow(sb, 62, 555, 471, 34, "Numero de visitantes / sesiones", visitors, false);
        DrawMetricRow(sb, 62, 505, 471, 34, "Duracion acumulada (segundos)", totalDuration, true);
        DrawMetricRow(sb, 62, 455, 471, 34, "Duracion promedio de sesion (segundos)", avgSession, false);
        DrawMetricRow(sb, 62, 405, 471, 34, "Numero de interacciones", interactions, true);
        DrawMetricRow(sb, 62, 355, 471, 34, "Etapas completadas", stages, false);

        DrawText(sb, "FBold", 13, 48, 305, "Interpretacion general", "#6B4A32");
        DrawText(sb, "FRegular", 9, 48, 286, "Estos datos permiten analizar el uso de la experiencia, la permanencia del usuario,", "#404040");
        DrawText(sb, "FRegular", 9, 48, 272, "las interacciones realizadas y el avance en las etapas culturales del sistema.", "#404040");
        DrawText(sb, "FRegular", 9, 48, 258, "La informacion recopilada apoya futuras mejoras de apropiacion cultural.", "#404040");

        DrawLine(sb, 48, 120, 547, 120, "#A36A3D", 1f);

        DrawText(sb, "FRegular", 10, 48, 96, "Fecha de generacion: " + date, "#6B4A32");
        DrawText(sb, "FRegular", 9, 48, 76, "Documento generado automaticamente desde el panel administrativo de InstaHistory.", "#404040");
        DrawText(sb, "FRegular", 9, 48, 58, "Universidad Autonoma de Occidente - Proyecto academico", "#A36A3D");

        if (hasUaoLogo)
        {
            FillRect(sb, 390, 40, 150, 34, "#FFFFFF");

            RectFit fit = FitInsideBox(uaoLogo.Width, uaoLogo.Height, 398, 44, 134, 22);
            DrawImage(sb, "ImUAO", fit.X, fit.Y, fit.Width, fit.Height);
        }

        return sb.ToString();
    }

    private MetricsSnapshot GetMetricsSnapshot()
{
    int visitors = PlayerPrefs.GetInt(TotalVisitorsKey, 0);
    float totalDuration = PlayerPrefs.GetFloat(TotalSessionDurationKey, 0f);
    int interactions = PlayerPrefs.GetInt(TotalInteractionsKey, 0);
    int stages = PlayerPrefs.GetInt(CompletedStagesKey, 0);

    float averageSession = visitors > 0 ? totalDuration / visitors : 0f;

    return new MetricsSnapshot
    {
        TotalVisitors = visitors,
        TotalSessionDuration = totalDuration,
        AverageSessionDuration = averageSession,
        TotalInteractions = interactions,
        CompletedStages = stages
    };
}

    private void DrawMetricRow(StringBuilder sb, float x, float y, float width, float height, string label, string value, bool alternate)
    {
        FillRect(sb, x, y, width, height, alternate ? "#F5EEDB" : "#F0DFC0");
        StrokeRect(sb, x, y, width, height, "#C08D52", 0.8f);

        DrawText(sb, "FBold", 10, x + 10, y + 12, label + ":", "#6B4A32");
        DrawText(sb, "FRegular", 11, x + 345, y + 12, value, "#017FBD");
    }

    private void DrawText(StringBuilder sb, string fontName, int fontSize, float x, float y, string text, string hexColor)
    {
        (float r, float g, float b) = HexToRgb(hexColor);

        sb.AppendLine("BT");
        sb.AppendLine($"/{fontName} {fontSize} Tf");
        sb.AppendLine($"{N(r)} {N(g)} {N(b)} rg");
        sb.AppendLine($"{N(x)} {N(y)} Td");
        sb.AppendLine($"({EscapePdfText(text)}) Tj");
        sb.AppendLine("ET");
    }

    private void FillRect(StringBuilder sb, float x, float y, float width, float height, string hexColor)
    {
        (float r, float g, float b) = HexToRgb(hexColor);
        sb.AppendLine($"{N(r)} {N(g)} {N(b)} rg");
        sb.AppendLine($"{N(x)} {N(y)} {N(width)} {N(height)} re f");
    }

    private void StrokeRect(StringBuilder sb, float x, float y, float width, float height, string hexColor, float lineWidth)
    {
        (float r, float g, float b) = HexToRgb(hexColor);
        sb.AppendLine($"{N(r)} {N(g)} {N(b)} RG");
        sb.AppendLine($"{N(lineWidth)} w");
        sb.AppendLine($"{N(x)} {N(y)} {N(width)} {N(height)} re S");
    }

    private void DrawLine(StringBuilder sb, float x1, float y1, float x2, float y2, string hexColor, float lineWidth)
    {
        (float r, float g, float b) = HexToRgb(hexColor);
        sb.AppendLine($"{N(r)} {N(g)} {N(b)} RG");
        sb.AppendLine($"{N(lineWidth)} w");
        sb.AppendLine($"{N(x1)} {N(y1)} m {N(x2)} {N(y2)} l S");
    }

    private void DrawImage(StringBuilder sb, string imageName, float x, float y, float width, float height)
    {
        sb.AppendLine("q");
        sb.AppendLine($"{N(width)} 0 0 {N(height)} {N(x)} {N(y)} cm");
        sb.AppendLine($"/{imageName} Do");
        sb.AppendLine("Q");
    }

    private RectFit FitInsideBox(int originalWidth, int originalHeight, float boxX, float boxY, float boxWidth, float boxHeight)
    {
        if (originalWidth <= 0 || originalHeight <= 0)
            return new RectFit { X = boxX, Y = boxY, Width = boxWidth, Height = boxHeight };

        float ratio = Mathf.Min(boxWidth / originalWidth, boxHeight / originalHeight);
        float width = originalWidth * ratio;
        float height = originalHeight * ratio;
        float x = boxX + (boxWidth - width) / 2f;
        float y = boxY + (boxHeight - height) / 2f;

        return new RectFit { X = x, Y = y, Width = width, Height = height };
    }

    private PdfRasterImage LoadPngAsJpgForPdf(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return null;

        string fullPath = Path.Combine(Application.streamingAssetsPath, "Logos", fileName);

        if (!File.Exists(fullPath))
        {
            UnityEngine.Debug.LogWarning("No se encontro el logo en: " + fullPath);
            return null;
        }

        byte[] originalBytes = File.ReadAllBytes(fullPath);

        Texture2D source = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        bool loaded = source.LoadImage(originalBytes);

        if (!loaded)
        {
            UnityEngine.Debug.LogWarning("No se pudo cargar la imagen: " + fullPath);
            Destroy(source);
            return null;
        }

        Texture2D flattened = new Texture2D(source.width, source.height, TextureFormat.RGB24, false);

        Color[] src = source.GetPixels();
        Color[] dst = new Color[src.Length];
        Color white = Color.white;

        for (int i = 0; i < src.Length; i++)
        {
            Color c = src[i];
            dst[i] = Color.Lerp(white, new Color(c.r, c.g, c.b, 1f), c.a);
        }

        flattened.SetPixels(dst);
        flattened.Apply();

        byte[] jpgBytes = flattened.EncodeToJPG(92);

        PdfRasterImage result = new PdfRasterImage
        {
            JpegBytes = jpgBytes,
            Width = flattened.width,
            Height = flattened.height
        };

        Destroy(source);
        Destroy(flattened);

        return result;
    }

    private static string N(float value)
    {
        return value.ToString("0.##", CultureInfo.InvariantCulture);
    }

    private (float r, float g, float b) HexToRgb(string hex)
    {
        hex = hex.Replace("#", "");
        if (hex.Length != 6) return (0f, 0f, 0f);

        byte r = byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        byte g = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        byte b = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);

        return (r / 255f, g / 255f, b / 255f);
    }

    private string EscapePdfText(string text)
    {
        if (string.IsNullOrEmpty(text))
            return "";

        return text
            .Replace("\\", "\\\\")
            .Replace("(", "\\(")
            .Replace(")", "\\)");
    }

    private class MetricsSnapshot
    {
        public int TotalVisitors;
        public float TotalSessionDuration;
        public float AverageSessionDuration;
        public int TotalInteractions;
        public int CompletedStages;
    }

    private class PdfRasterImage
    {
        public byte[] JpegBytes;
        public int Width;
        public int Height;
    }

    private class RectFit
    {
        public float X;
        public float Y;
        public float Width;
        public float Height;
    }
}