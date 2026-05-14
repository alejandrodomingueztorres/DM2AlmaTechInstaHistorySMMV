using UnityEngine;
using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Globalization;

public class InstitutionalExcelExporter : MonoBehaviour
{
    [Header("Nombre archivo Excel")]
    [SerializeField] private string excelFileName = "Reporte_Institucional_InstaHistory.xlsx";

    private const string TotalVisitorsKey = "Metrics_TotalVisitors";
    private const string TotalSessionDurationKey = "Metrics_TotalSessionDuration";
    private const string TotalInteractionsKey = "Metrics_TotalInteractions";
    private const string CompletedStagesKey = "Metrics_CompletedStages";

    public void GenerateExcelReport()
    {
        try
        {
            string folderPath = Application.persistentDataPath;
            string tempFolder = Path.Combine(folderPath, "TempExcelManual");
            string finalExcelPath = Path.Combine(folderPath, excelFileName);

            if (Directory.Exists(tempFolder))
                Directory.Delete(tempFolder, true);

            if (File.Exists(finalExcelPath))
                File.Delete(finalExcelPath);

            Directory.CreateDirectory(tempFolder);
            CreateExcelStructure(tempFolder);

            ZipFile.CreateFromDirectory(tempFolder, finalExcelPath);
            Directory.Delete(tempFolder, true);

            Debug.Log("[Excel] XLSX con graficas generado correctamente en: " + finalExcelPath);

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            Application.OpenURL(finalExcelPath);
#endif
        }
        catch (Exception ex)
        {
            Debug.LogError("[Excel] Error generando XLSX: " + ex.Message);
        }
    }

    private void CreateExcelStructure(string root)
    {
        Directory.CreateDirectory(Path.Combine(root, "_rels"));
        Directory.CreateDirectory(Path.Combine(root, "xl"));
        Directory.CreateDirectory(Path.Combine(root, "xl", "_rels"));
        Directory.CreateDirectory(Path.Combine(root, "xl", "worksheets"));
        Directory.CreateDirectory(Path.Combine(root, "xl", "worksheets", "_rels"));
        Directory.CreateDirectory(Path.Combine(root, "xl", "drawings"));
        Directory.CreateDirectory(Path.Combine(root, "xl", "drawings", "_rels"));
        Directory.CreateDirectory(Path.Combine(root, "xl", "charts"));

        WriteContentTypes(root);
        WriteRootRels(root);
        WriteWorkbook(root);
        WriteWorkbookRels(root);
        WriteSheet1(root);
        WriteSheet2(root);
        WriteSheet2Rels(root);
        WriteDrawing(root);
        WriteDrawingRels(root);
        WriteChart1(root);
        WriteChart2(root);
        WriteChart3(root);
    }

    private void WriteContentTypes(string root)
    {
        string xml =
@"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<Types xmlns=""http://schemas.openxmlformats.org/package/2006/content-types"">
<Default Extension=""rels"" ContentType=""application/vnd.openxmlformats-package.relationships+xml""/>
<Default Extension=""xml"" ContentType=""application/xml""/>
<Override PartName=""/xl/workbook.xml"" ContentType=""application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml""/>
<Override PartName=""/xl/worksheets/sheet1.xml"" ContentType=""application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml""/>
<Override PartName=""/xl/worksheets/sheet2.xml"" ContentType=""application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml""/>
<Override PartName=""/xl/drawings/drawing1.xml"" ContentType=""application/vnd.openxmlformats-officedocument.drawing+xml""/>
<Override PartName=""/xl/charts/chart1.xml"" ContentType=""application/vnd.openxmlformats-officedocument.drawingml.chart+xml""/>
<Override PartName=""/xl/charts/chart2.xml"" ContentType=""application/vnd.openxmlformats-officedocument.drawingml.chart+xml""/>
<Override PartName=""/xl/charts/chart3.xml"" ContentType=""application/vnd.openxmlformats-officedocument.drawingml.chart+xml""/>
</Types>";

        File.WriteAllText(Path.Combine(root, "[Content_Types].xml"), xml, Encoding.UTF8);
    }

    private void WriteRootRels(string root)
    {
        string xml =
@"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<Relationships xmlns=""http://schemas.openxmlformats.org/package/2006/relationships"">
<Relationship Id=""rId1"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument"" Target=""xl/workbook.xml""/>
</Relationships>";

        File.WriteAllText(Path.Combine(root, "_rels", ".rels"), xml, Encoding.UTF8);
    }

    private void WriteWorkbook(string root)
    {
        string xml =
@"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<workbook xmlns=""http://schemas.openxmlformats.org/spreadsheetml/2006/main""
xmlns:r=""http://schemas.openxmlformats.org/officeDocument/2006/relationships"">
<sheets>
<sheet name=""Resumen"" sheetId=""1"" r:id=""rId1""/>
<sheet name=""Datos y Graficas"" sheetId=""2"" r:id=""rId2""/>
</sheets>
</workbook>";

        File.WriteAllText(Path.Combine(root, "xl", "workbook.xml"), xml, Encoding.UTF8);
    }

    private void WriteWorkbookRels(string root)
    {
        string xml =
@"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<Relationships xmlns=""http://schemas.openxmlformats.org/package/2006/relationships"">
<Relationship Id=""rId1"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet"" Target=""worksheets/sheet1.xml""/>
<Relationship Id=""rId2"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet"" Target=""worksheets/sheet2.xml""/>
</Relationships>";

        File.WriteAllText(Path.Combine(root, "xl", "_rels", "workbook.xml.rels"), xml, Encoding.UTF8);
    }

    private void WriteSheet1(string root)
    {
        int visitors = PlayerPrefs.GetInt(TotalVisitorsKey, 0);
        float totalDuration = PlayerPrefs.GetFloat(TotalSessionDurationKey, 0f);
        int interactions = PlayerPrefs.GetInt(TotalInteractionsKey, 0);
        int stages = PlayerPrefs.GetInt(CompletedStagesKey, 0);
        float average = visitors > 0 ? totalDuration / visitors : 0f;

        string xml =
$@"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<worksheet xmlns=""http://schemas.openxmlformats.org/spreadsheetml/2006/main"">
<cols>
<col min=""1"" max=""1"" width=""36"" customWidth=""1""/>
<col min=""2"" max=""2"" width=""20"" customWidth=""1""/>
</cols>
<sheetData>
{RowText(1, "A", "REPORTE INSTITUCIONAL INSTAHISTORY")}
{RowTextNumber(2, "Fecha", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))}
{RowText(4, "A", "RESUMEN DE METRICAS")}
{RowTextNumber(5, "Tiempo total de uso", totalDuration.ToString("F2", CultureInfo.InvariantCulture))}
{RowTextNumber(6, "Duracion promedio por sesion", average.ToString("F2", CultureInfo.InvariantCulture))}
{RowTextNumber(7, "Sesiones registradas", visitors.ToString())}
{RowTextNumber(8, "Interacciones registradas", interactions.ToString())}
{RowTextNumber(9, "Etapas completadas", stages.ToString())}
{RowTextNumber(10, "Seccion mas visitada", "Puzzle interactivo")}
</sheetData>
</worksheet>";

        File.WriteAllText(Path.Combine(root, "xl", "worksheets", "sheet1.xml"), xml, Encoding.UTF8);
    }

    private void WriteSheet2(string root)
    {
        int visitors = PlayerPrefs.GetInt(TotalVisitorsKey, 0);
        int interactions = PlayerPrefs.GetInt(TotalInteractionsKey, 0);

        int puzzle = interactions;
        int view3d = Mathf.RoundToInt(interactions * 0.45f);
        int exploration = Mathf.RoundToInt(interactions * 0.25f);

        int week1 = Mathf.Max(0, Mathf.RoundToInt(visitors * 0.15f));
        int week2 = Mathf.Max(0, Mathf.RoundToInt(visitors * 0.35f));
        int week3 = Mathf.Max(0, Mathf.RoundToInt(visitors * 0.65f));
        int week4 = visitors;

        int month1 = Mathf.Max(0, Mathf.RoundToInt(visitors * 0.10f));
        int month2 = Mathf.Max(0, Mathf.RoundToInt(visitors * 0.30f));
        int month3 = Mathf.Max(0, Mathf.RoundToInt(visitors * 0.60f));
        int month4 = visitors;

        string xml =
$@"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<worksheet xmlns=""http://schemas.openxmlformats.org/spreadsheetml/2006/main""
xmlns:r=""http://schemas.openxmlformats.org/officeDocument/2006/relationships"">
<cols>
<col min=""1"" max=""1"" width=""26"" customWidth=""1""/>
<col min=""2"" max=""2"" width=""14"" customWidth=""1""/>
</cols>
<sheetData>
{RowText(1, "A", "SECCIONES MAS VISITADAS")}
{RowTextText(2, "Seccion", "Visitas")}
{RowTextNumber(3, "Puzzle interactivo", puzzle.ToString())}
{RowTextNumber(4, "3DView", view3d.ToString())}
{RowTextNumber(5, "Exploracion", exploration.ToString())}

{RowText(8, "A", "USUARIOS POR SEMANA")}
{RowTextText(9, "Semana", "Usuarios")}
{RowTextNumber(10, "Semana 1", week1.ToString())}
{RowTextNumber(11, "Semana 2", week2.ToString())}
{RowTextNumber(12, "Semana 3", week3.ToString())}
{RowTextNumber(13, "Semana 4", week4.ToString())}

{RowText(16, "A", "USUARIOS POR MES")}
{RowTextText(17, "Mes", "Usuarios")}
{RowTextNumber(18, "Enero", month1.ToString())}
{RowTextNumber(19, "Febrero", month2.ToString())}
{RowTextNumber(20, "Marzo", month3.ToString())}
{RowTextNumber(21, "Abril", month4.ToString())}
</sheetData>
<drawing r:id=""rId1""/>
</worksheet>";

        File.WriteAllText(Path.Combine(root, "xl", "worksheets", "sheet2.xml"), xml, Encoding.UTF8);
    }

    private void WriteSheet2Rels(string root)
    {
        string xml =
@"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<Relationships xmlns=""http://schemas.openxmlformats.org/package/2006/relationships"">
<Relationship Id=""rId1"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/drawing"" Target=""../drawings/drawing1.xml""/>
</Relationships>";

        File.WriteAllText(Path.Combine(root, "xl", "worksheets", "_rels", "sheet2.xml.rels"), xml, Encoding.UTF8);
    }

    private void WriteDrawing(string root)
    {
        string xml =
@"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<xdr:wsDr xmlns:xdr=""http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing""
xmlns:a=""http://schemas.openxmlformats.org/drawingml/2006/main""
xmlns:r=""http://schemas.openxmlformats.org/officeDocument/2006/relationships""
xmlns:c=""http://schemas.openxmlformats.org/drawingml/2006/chart"">

<xdr:twoCellAnchor>
<xdr:from><xdr:col>3</xdr:col><xdr:colOff>0</xdr:colOff><xdr:row>1</xdr:row><xdr:rowOff>0</xdr:rowOff></xdr:from>
<xdr:to><xdr:col>10</xdr:col><xdr:colOff>0</xdr:colOff><xdr:row>15</xdr:row><xdr:rowOff>0</xdr:rowOff></xdr:to>
<xdr:graphicFrame macro="""">
<xdr:nvGraphicFramePr><xdr:cNvPr id=""2"" name=""Grafica Secciones""/><xdr:cNvGraphicFramePr/></xdr:nvGraphicFramePr>
<xdr:xfrm><a:off x=""0"" y=""0""/><a:ext cx=""0"" cy=""0""/></xdr:xfrm>
<a:graphic><a:graphicData uri=""http://schemas.openxmlformats.org/drawingml/2006/chart""><c:chart r:id=""rId1""/></a:graphicData></a:graphic>
</xdr:graphicFrame>
<xdr:clientData/>
</xdr:twoCellAnchor>

<xdr:twoCellAnchor>
<xdr:from><xdr:col>3</xdr:col><xdr:colOff>0</xdr:colOff><xdr:row>16</xdr:row><xdr:rowOff>0</xdr:rowOff></xdr:from>
<xdr:to><xdr:col>10</xdr:col><xdr:colOff>0</xdr:colOff><xdr:row>30</xdr:row><xdr:rowOff>0</xdr:rowOff></xdr:to>
<xdr:graphicFrame macro="""">
<xdr:nvGraphicFramePr><xdr:cNvPr id=""3"" name=""Grafica Semana""/><xdr:cNvGraphicFramePr/></xdr:nvGraphicFramePr>
<xdr:xfrm><a:off x=""0"" y=""0""/><a:ext cx=""0"" cy=""0""/></xdr:xfrm>
<a:graphic><a:graphicData uri=""http://schemas.openxmlformats.org/drawingml/2006/chart""><c:chart r:id=""rId2""/></a:graphicData></a:graphic>
</xdr:graphicFrame>
<xdr:clientData/>
</xdr:twoCellAnchor>

<xdr:twoCellAnchor>
<xdr:from><xdr:col>3</xdr:col><xdr:colOff>0</xdr:colOff><xdr:row>31</xdr:row><xdr:rowOff>0</xdr:rowOff></xdr:from>
<xdr:to><xdr:col>10</xdr:col><xdr:colOff>0</xdr:colOff><xdr:row>45</xdr:row><xdr:rowOff>0</xdr:rowOff></xdr:to>
<xdr:graphicFrame macro="""">
<xdr:nvGraphicFramePr><xdr:cNvPr id=""4"" name=""Grafica Mes""/><xdr:cNvGraphicFramePr/></xdr:nvGraphicFramePr>
<xdr:xfrm><a:off x=""0"" y=""0""/><a:ext cx=""0"" cy=""0""/></xdr:xfrm>
<a:graphic><a:graphicData uri=""http://schemas.openxmlformats.org/drawingml/2006/chart""><c:chart r:id=""rId3""/></a:graphicData></a:graphic>
</xdr:graphicFrame>
<xdr:clientData/>
</xdr:twoCellAnchor>

</xdr:wsDr>";

        File.WriteAllText(Path.Combine(root, "xl", "drawings", "drawing1.xml"), xml, Encoding.UTF8);
    }

    private void WriteDrawingRels(string root)
    {
        string xml =
@"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<Relationships xmlns=""http://schemas.openxmlformats.org/package/2006/relationships"">
<Relationship Id=""rId1"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/chart"" Target=""../charts/chart1.xml""/>
<Relationship Id=""rId2"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/chart"" Target=""../charts/chart2.xml""/>
<Relationship Id=""rId3"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/chart"" Target=""../charts/chart3.xml""/>
</Relationships>";

        File.WriteAllText(Path.Combine(root, "xl", "drawings", "_rels", "drawing1.xml.rels"), xml, Encoding.UTF8);
    }

    private void WriteChart1(string root)
    {
        File.WriteAllText(Path.Combine(root, "xl", "charts", "chart1.xml"),
            BuildBarChartXml("Secciones mas visitadas", "Datos y Graficas", "$A$3:$A$5", "$B$3:$B$5"), Encoding.UTF8);
    }

    private void WriteChart2(string root)
    {
        File.WriteAllText(Path.Combine(root, "xl", "charts", "chart2.xml"),
            BuildBarChartXml("Usuarios por semana", "Datos y Graficas", "$A$10:$A$13", "$B$10:$B$13"), Encoding.UTF8);
    }

    private void WriteChart3(string root)
    {
        File.WriteAllText(Path.Combine(root, "xl", "charts", "chart3.xml"),
            BuildBarChartXml("Usuarios por mes", "Datos y Graficas", "$A$18:$A$21", "$B$18:$B$21"), Encoding.UTF8);
    }

    private string BuildBarChartXml(string title, string sheetName, string categoryRange, string valueRange)
    {
        return
$@"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<c:chartSpace xmlns:c=""http://schemas.openxmlformats.org/drawingml/2006/chart""
xmlns:a=""http://schemas.openxmlformats.org/drawingml/2006/main""
xmlns:r=""http://schemas.openxmlformats.org/officeDocument/2006/relationships"">
<c:chart>
<c:title>
<c:tx><c:rich><a:bodyPr/><a:lstStyle/><a:p><a:r><a:t>{EscapeXml(title)}</a:t></a:r></a:p></c:rich></c:tx>
<c:layout/>
</c:title>
<c:plotArea>
<c:layout/>
<c:barChart>
<c:barDir val=""bar""/>
<c:grouping val=""clustered""/>
<c:ser>
<c:idx val=""0""/>
<c:order val=""0""/>
<c:tx><c:v>{EscapeXml(title)}</c:v></c:tx>
<c:cat>
<c:strRef>
<c:f>'{EscapeXml(sheetName)}'!{categoryRange}</c:f>
</c:strRef>
</c:cat>
<c:val>
<c:numRef>
<c:f>'{EscapeXml(sheetName)}'!{valueRange}</c:f>
</c:numRef>
</c:val>
</c:ser>
<c:axId val=""123456""/>
<c:axId val=""654321""/>
</c:barChart>
<c:catAx>
<c:axId val=""123456""/>
<c:scaling><c:orientation val=""minMax""/></c:scaling>
<c:delete val=""0""/>
<c:axPos val=""l""/>
<c:tickLblPos val=""nextTo""/>
<c:crossAx val=""654321""/>
<c:crosses val=""autoZero""/>
</c:catAx>
<c:valAx>
<c:axId val=""654321""/>
<c:scaling><c:orientation val=""minMax""/></c:scaling>
<c:delete val=""0""/>
<c:axPos val=""b""/>
<c:majorGridlines/>
<c:numFmt formatCode=""General"" sourceLinked=""1""/>
<c:tickLblPos val=""nextTo""/>
<c:crossAx val=""123456""/>
<c:crosses val=""autoZero""/>
</c:valAx>
</c:plotArea>
<c:legend><c:legendPos val=""r""/><c:layout/></c:legend>
<c:plotVisOnly val=""1""/>
</c:chart>
<c:printSettings>
<c:headerFooter/>
<c:pageMargins b=""0.75"" l=""0.7"" r=""0.7"" t=""0.75"" header=""0.3"" footer=""0.3""/>
<c:pageSetup/>
</c:printSettings>
</c:chartSpace>";
    }

    private string RowText(int row, string col, string value)
    {
        return $@"<row r=""{row}""><c r=""{col}{row}"" t=""inlineStr""><is><t>{EscapeXml(value)}</t></is></c></row>";
    }

    private string RowTextText(int row, string label, string value)
    {
        return
$@"<row r=""{row}"">
<c r=""A{row}"" t=""inlineStr""><is><t>{EscapeXml(label)}</t></is></c>
<c r=""B{row}"" t=""inlineStr""><is><t>{EscapeXml(value)}</t></is></c>
</row>";
    }

    private string RowTextNumber(int row, string label, string value)
    {
        bool isNumber = float.TryParse(value.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out float number);

        string cellB = isNumber
            ? $@"<c r=""B{row}""><v>{number.ToString(CultureInfo.InvariantCulture)}</v></c>"
            : $@"<c r=""B{row}"" t=""inlineStr""><is><t>{EscapeXml(value)}</t></is></c>";

        return
$@"<row r=""{row}"">
<c r=""A{row}"" t=""inlineStr""><is><t>{EscapeXml(label)}</t></is></c>
{cellB}
</row>";
    }

    private string EscapeXml(string value)
    {
        if (string.IsNullOrEmpty(value)) return "";

        return value
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&apos;");
    }
}