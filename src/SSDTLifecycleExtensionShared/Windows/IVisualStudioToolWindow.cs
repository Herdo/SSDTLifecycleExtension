namespace SSDTLifecycleExtension.Windows;

public interface IVisualStudioToolWindow
{
    object Content { get; set; }

    void SetCaption(string projectName);
}