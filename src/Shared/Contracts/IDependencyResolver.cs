namespace SSDTLifecycleExtension.Shared.Contracts;

public interface IDependencyResolver : IDisposable
{
    /// <summary>
    ///     Gets an instance for the given <typeparamref name="TTypeToGet" />.
    /// </summary>
    /// <typeparam name="TTypeToGet">The <see cref="Type" /> to get.</typeparam>
    /// <exception cref="ObjectDisposedException">The <see cref="IDependencyResolver" /> instance has been disposed.</exception>
    /// <returns>An instance of <typeparamref name="TTypeToGet" />.</returns>
    TTypeToGet Get<TTypeToGet>();
}