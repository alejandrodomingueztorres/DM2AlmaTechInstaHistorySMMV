using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

public class InstitutionalPDFReportGenerator : MonoBehaviour
{
    [Header("Archivo PDF")]
    [SerializeField] private string pdfFileName = "Reporte_Institucional_InstaHistory.pdf";

    [Header("Logos PNG en Assets/StreamingAssets/Logos")]
    [SerializeField] private string projectLogoFileName = "logo_instahistory.png";
    [SerializeField] private string universityLogoFileName = "logo_uao.png";

    private const string TotalVisitorsKey = "Metrics_TotalVisitors";
    private const string TotalSessionDurationKey = "Metrics_TotalSessionDuration";
    private const string TotalInteractionsKey = "Metrics_TotalInteractions";
    private const string CompletedStagesKey = "Metrics_CompletedStages";

    public void GenerateInstitutionalReport()
    {
        string filePath = Path.Combine(Application.persistentDataPath, pdfFileName);

        try
        {
            byte[] pdfBytes = BuildPdf();
            File.WriteAllBytes(filePath, pdfBytes);

            UnityEngine.Debug.Log("[Institutional PDF] Reporte generado correctamente en: " + filePath);

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            Application.OpenURL(filePath);
#endif
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError("[Institutional PDF] Error al generar reporte: " + ex.Message);
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

        UnityEngine.Debug.Log("[Institutional PDF] Métricas reiniciadas correctamente.");
    }

    private byte[] BuildPdf()
    {
        using MemoryStream ms = new MemoryStream();
        List<long> offsets = new List<long>();

        PdfRasterImage projectLogo = LoadPngAsJpgForPdf(projectLogoFileName);
        PdfRasterImage uaoLogo = LoadPngAsJpgForPdf(universityLogoFileName);

        bool hasProjectLogo = projectLogo != null;
        bool hasUaoLogo = uaoLogo != null;

        int visitors = PlayerPrefs.GetInt(TotalVisitorsKey, 0);
        float totalDuration = PlayerPrefs.GetFloat(TotalSessionDurationKey, 0f);
        int interactions = PlayerPrefs.GetInt(TotalInteractionsKey, 0);
        int stages = PlayerPrefs.GetInt(CompletedStagesKey, 0);

        float avgDuration = visitors > 0 ? totalDuration / visitors : 0f;
        string mostVisitedSection = stages > 0 ? "Puzzle interactivo" : "Sin datos suficientes";

        void W(string value)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(value);
            ms.Write(bytes, 0, bytes.Length);
        }

        void WB(byte[] bytes)
        {
            ms.Write(bytes, 0, bytes.Length);
        }

        void Obj(int number, string content)
        {
            offsets.Add(ms.Position);
            W(number + " 0 obj\n");
            W(content);
            W("\nendobj\n");
        }

        W("%PDF-1.4\n");

        Obj(1, "<< /Type /Catalog /Pages 2 0 R >>");
        Obj(2, "<< /Type /Pages /Kids [3 0 R] /Count 1 >>");

        string xObjects = "";
        int nextImageObject = 7;

        if (hasProjectLogo) xObjects += "/ImProject " + nextImageObject++ + " 0 R ";
        if (hasUaoLogo) xObjects += "/ImUAO " + nextImageObject++ + " 0 R ";

        string pageResources =
            "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Contents 4 0 R " +
            "/Resources << /Font << /F1 5 0 R /F2 6 0 R >> ";

        if (!string.IsNullOrEmpty(xObjects))
            pageResources += "/XObject << " + xObjects + ">> ";

        pageResources += ">> >>";

        Obj(3, pageResources);

        string content = BuildContent(
            visitors,
            totalDuration,
            avgDuration,
            interactions,
            stages,
            mostVisitedSection,
            hasProjectLogo,
            hasUaoLogo,
            projectLogo,
            uaoLogo
        );

        byte[] contentBytes = Encoding.ASCII.GetBytes(content);

        offsets.Add(ms.Position);
        W("4 0 obj\n");
        W("<< /Length " + contentBytes.Length + " >>\n");
        W("stream\n");
        WB(contentBytes);
        W("\nendstream\nendobj\n");

        Obj(5, "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>");
        Obj(6, "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold >>");

        int objectId = 7;

        if (hasProjectLogo)
            WriteImageObject(objectId++, projectLogo);

        if (hasUaoLogo)
            WriteImageObject(objectId++, uaoLogo);

        long xref = ms.Position;

        W("xref\n");
        W("0 " + objectId + "\n");
        W("0000000000 65535 f \n");

        foreach (long offset in offsets)
            W(offset.ToString("D10", CultureInfo.InvariantCulture) + " 00000 n \n");

        W("trailer\n");
        W("<< /Size " + objectId + " /Root 1 0 R >>\n");
        W("startxref\n");
        W(xref.ToString(CultureInfo.InvariantCulture) + "\n");
        W("%%EOF");

        return ms.ToArray();

        void WriteImageObject(int number, PdfRasterImage img)
        {
            offsets.Add(ms.Position);
            W(number + " 0 obj\n");
            W("<< /Type /XObject /Subtype /Image ");
            W("/Width " + img.Width + " ");
            W("/Height " + img.Height + " ");
            W("/ColorSpace /DeviceRGB ");
            W("/BitsPerComponent 8 ");
            W("/Filter /DCTDecode ");
            W("/Length " + img.JpegBytes.Length + " >>\n");
            W("stream\n");
            WB(img.JpegBytes);
            W("\nendstream\nendobj\n");
        }
    }

    private string BuildContent(
        int visitors,
        float totalDuration,
        float avgDuration,
        int interactions,
        int stages,
        string mostVisitedSection,
        bool hasProjectLogo,
        bool hasUaoLogo,
        PdfRasterImage projectLogo,
        PdfRasterImage uaoLogo)
    {
        StringBuilder sb = new StringBuilder();

        FillRect(sb, 0, 0, 595, 842, "FFFFFF");
        StrokeRect(sb, 18, 18, 559, 806, "6B4A32", 1.8f);

        FillRect(sb, 18, 720, 559, 104, "055169");
        FillRect(sb, 18, 710, 559, 10, "017FBD");
        FillRect(sb, 18, 704, 559, 6, "D19525");

        if (hasProjectLogo)
        {
            FillRect(sb, 34, 740, 120, 50, "FFFFFF");
            StrokeRect(sb, 34, 740, 120, 50, "E5D3AD", 1f);

            RectFit fit = FitInsideBox(projectLogo.Width, projectLogo.Height, 40, 744, 108, 42);
            DrawImage(sb, "ImProject", fit.X, fit.Y, fit.Width, fit.Height);
        }

        DrawText(sb, "F2", 22, 180, 780, "Reporte institucional", "FFFFFF");
        DrawText(sb, "F1", 10, 180, 760, "Uso e impacto academico del sistema", "E5D3AD");

        DrawText(sb, "F2", 15, 48, 660, "Resumen de metricas institucionales", "6B4A32");
        DrawLine(sb, 48, 648, 547, 648, "4FB3BF", 1.2f);

        DrawMetricRow(sb, 48, 605, "Tiempo total de uso", totalDuration.ToString("F2", CultureInfo.InvariantCulture) + " segundos");
        DrawMetricRow(sb, 48, 560, "Duracion promedio por sesion", avgDuration.ToString("F2", CultureInfo.InvariantCulture) + " segundos");
        DrawMetricRow(sb, 48, 515, "Seccion mas visitada", mostVisitedSection);
        DrawMetricRow(sb, 48, 470, "Sesiones registradas", visitors.ToString());
        DrawMetricRow(sb, 48, 425, "Interacciones registradas", interactions.ToString());
        DrawMetricRow(sb, 48, 380, "Etapas completadas", stages.ToString());

        DrawText(sb, "F2", 14, 48, 320, "Lectura academica", "6B4A32");
        DrawText(sb, "F1", 9, 48, 300, "El reporte permite evaluar el comportamiento de uso del sistema y su potencial", "404040");
        DrawText(sb, "F1", 9, 48, 286, "aporte a procesos de apropiacion cultural y aprendizaje mediante interaccion.", "404040");
        DrawText(sb, "F1", 9, 48, 272, "Las metricas sirven como base para analizar permanencia, participacion y avance.", "404040");

        DrawText(sb, "F2", 14, 48, 230, "Observacion", "6B4A32");
        DrawText(sb, "F1", 9, 48, 210, "Actualmente la seccion mas visitada se estima a partir de los eventos registrados", "404040");
        DrawText(sb, "F1", 9, 48, 196, "en el modulo Puzzle, dejando la estructura preparada para integrar 3DView y", "404040");
        DrawText(sb, "F1", 9, 48, 182, "otros modulos cuando el flujo completo de escenas sea conectado.", "404040");

        DrawLine(sb, 48, 120, 547, 120, "A36A3D", 1f);

        DrawText(sb, "F1", 9, 48, 96, "Fecha de generacion: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "6B4A32");
        DrawText(sb, "F1", 9, 48, 76, "Reporte generado para usuario institucional.", "404040");
        DrawText(sb, "F1", 9, 48, 58, "Universidad Autonoma de Occidente - Proyecto academico", "A36A3D");

        if (hasUaoLogo)
        {
            RectFit fit = FitInsideBox(uaoLogo.Width, uaoLogo.Height, 395, 42, 135, 35);
            DrawImage(sb, "ImUAO", fit.X, fit.Y, fit.Width, fit.Height);
        }

        return sb.ToString();
    }

    private void DrawMetricRow(StringBuilder sb, float x, float y, string label, string value)
    {
        FillRect(sb, x, y, 499, 30, "F0DFC0");
        StrokeRect(sb, x, y, 499, 30, "C08D52", 0.8f);

        DrawText(sb, "F2", 9, x + 10, y + 11, label + ":", "6B4A32");
        DrawText(sb, "F1", 9, x + 310, y + 11, value, "017FBD");
    }

    private PdfRasterImage LoadPngAsJpgForPdf(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return null;

        string fullPath = Path.Combine(Application.streamingAssetsPath, "Logos", fileName);

        if (!File.Exists(fullPath))
        {
            UnityEngine.Debug.LogWarning("[Institutional PDF] No se encontro el logo en: " + fullPath);
            return null;
        }

        byte[] originalBytes = File.ReadAllBytes(fullPath);

        Texture2D source = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        bool loaded = source.LoadImage(originalBytes);

        if (!loaded)
        {
            UnityEngine.Debug.LogWarning("[Institutional PDF] No se pudo cargar la imagen: " + fullPath);
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

    private RectFit FitInsideBox(int originalWidth, int originalHeight, float boxX, float boxY, float boxWidth, float boxHeight)
    {
        if (originalWidth <= 0 || originalHeight <= 0)
            return new RectFit { X = boxX, Y = boxY, Width = boxWidth, Height = boxHeight };

        float ratio = Mathf.Min(boxWidth / originalWidth, boxHeight / originalHeight);
        float width = originalWidth * ratio;
        float height = originalHeight * ratio;

        return new RectFit
        {
            X = boxX + (boxWidth - width) / 2f,
            Y = boxY + (boxHeight - height) / 2f,
            Width = width,
            Height = height
        };
    }

    private void DrawImage(StringBuilder sb, string imageName, float x, float y, float width, float height)
    {
        sb.AppendLine("q");
        sb.AppendLine($"{N(width)} 0 0 {N(height)} {N(x)} {N(y)} cm");
        sb.AppendLine($"/{imageName} Do");
        sb.AppendLine("Q");
    }

    private void DrawText(StringBuilder sb, string font, int size, float x, float y, string text, string hex)
    {
        (float r, float g, float b) = HexToRgb(hex);

        sb.AppendLine("BT");
        sb.AppendLine($"/{font} {size} Tf");
        sb.AppendLine($"{N(r)} {N(g)} {N(b)} rg");
        sb.AppendLine($"{N(x)} {N(y)} Td");
        sb.AppendLine($"({Escape(text)}) Tj");
        sb.AppendLine("ET");
    }

    private void FillRect(StringBuilder sb, float x, float y, float w, float h, string hex)
    {
        (float r, float g, float b) = HexToRgb(hex);
        sb.AppendLine($"{N(r)} {N(g)} {N(b)} rg");
        sb.AppendLine($"{N(x)} {N(y)} {N(w)} {N(h)} re f");
    }

    private void StrokeRect(StringBuilder sb, float x, float y, float w, float h, string hex, float lineWidth)
    {
        (float r, float g, float b) = HexToRgb(hex);
        sb.AppendLine($"{N(r)} {N(g)} {N(b)} RG");
        sb.AppendLine($"{N(lineWidth)} w");
        sb.AppendLine($"{N(x)} {N(y)} {N(w)} {N(h)} re S");
    }

    private void DrawLine(StringBuilder sb, float x1, float y1, float x2, float y2, string hex, float lineWidth)
    {
        (float r, float g, float b) = HexToRgb(hex);
        sb.AppendLine($"{N(r)} {N(g)} {N(b)} RG");
        sb.AppendLine($"{N(lineWidth)} w");
        sb.AppendLine($"{N(x1)} {N(y1)} m {N(x2)} {N(y2)} l S");
    }

    private (float r, float g, float b) HexToRgb(string hex)
    {
        hex = hex.Replace("#", "");

        byte r = byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
        byte g = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
        byte b = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);

        return (r / 255f, g / 255f, b / 255f);
    }

    private static string N(float value)
    {
        return value.ToString("0.##", CultureInfo.InvariantCulture);
    }

    private string Escape(string text)
    {
        if (string.IsNullOrEmpty(text))
            return "";

        return text.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");
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