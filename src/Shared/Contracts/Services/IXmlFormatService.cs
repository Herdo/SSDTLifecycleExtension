namespace SSDTLifecycleExtension.Shared.Contracts.Services;

public interface IXmlFormatService
{
    /// <summary>
    ///     Formats the <paramref name="report" /> with line breaks and white spaces.
    /// </summary>
    /// <param name="report">The report to format.</param>
    /// <exception cref="XmlException"><paramref name="report" /> is no valid xml.</exception>
    /// <returns><b>null</b>, if <paramref name="report" /> is <b>null</b>, otherwise the formatted report.</returns>
    string? FormatDeployReport(string? report);
}