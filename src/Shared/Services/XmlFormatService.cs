namespace SSDTLifecycleExtension.Shared.Services;

[UsedImplicitly]
public class XmlFormatService : IXmlFormatService
{
    /// <summary>
    ///     Formats the <paramref name="report" /> with line breaks and white spaces.
    /// </summary>
    /// <param name="report">The report to format.</param>
    /// <exception cref="XmlException"><paramref name="report" /> is no valid xml.</exception>
    /// <returns><b>null</b>, if <paramref name="report" /> is <b>null</b>, otherwise the formatted report.</returns>
    string IXmlFormatService.FormatDeployReport(string report)
    {
        if (report == null)
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