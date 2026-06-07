using AppCoel.Models.Auth;

namespace AppCoel.Core.Services.Auth
{
    public interface IPermissionService : IScopedServices
    {
        Task ValidadeCurrentUserPermissionAsync(PermissionType permissionType, PermissionContext? context = null, CancellationToken cancellationToken = default);
        Task<bool> HasCurrentUserPermissionAsync(PermissionType permissionType, PermissionContext? context = null, CancellationToken cancellationToken = default);
        Task<IEnumerable<PermissionType>> GetCurrentUserPermissionAsync(PermissionContext? context = null, CancellationToken cancellationToken = default);
    }
}
