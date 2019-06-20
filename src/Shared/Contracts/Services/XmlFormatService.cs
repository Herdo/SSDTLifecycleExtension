namespace SSDTLifecycleExtension.Shared.Contracts.Services
{
    using System.Text;
    using System.Xml;
    using System.Xml.Linq;
    using JetBrains.Annotations;

    [UsedImplicitly]
    public class XmlFormatService : IXmlFormatService
    {
        string IXmlFormatService.FormatDeployReport(string report)
        {
            if (report == null)
                return null;

            var doc = XDocument.Parse(report);
            var sb = new StringBuilder();
            var settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "    ",
                NewLineChars = "\r\n",
                NewLineHandling = NewLineHandling.Replace
            };

            using (var writer = XmlWriter.Create(sb, settings))
                doc.Save(writer);

            return sb.ToString();
        }
    }
}