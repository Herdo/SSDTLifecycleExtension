namespace SSDTLifecycleExtension.Shared.Services;

public class XmlFormatService : IXmlFormatService
{
    string? IXmlFormatService.FormatDeployReport(string? report)
    {
        if (report is null)
            return null;

        var doc = XDocument.Parse(report);
        var settings = new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "    ",
            NewLineChars = "\r\n",
            NewLineHandling = NewLineHandling.Replace
        };

        using var stringWriter = new Utf8StringWriter();
        using (var xmlWriter = XmlWriter.Create(stringWriter, settings))
        {
            doc.Save(xmlWriter);
        }

        return stringWriter.ToString();
    }

    private class Utf8StringWriter : StringWriter
    {
        public override Encoding Encoding => Encoding.UTF8;
    }
}