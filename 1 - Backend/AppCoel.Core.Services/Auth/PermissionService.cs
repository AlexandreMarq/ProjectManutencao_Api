using AppCoel.Core.Contracts;
using AppCoel.Exceptions;
using AppCoel.Models.Auth;

namespace AppCoel.Core.Services.Auth
{
    public class PermissionService(IUserContext userContext) : BaseService, IPermissionService
    {
        public async Task ValidadeCurrentUserPermissionAsync(PermissionType permissionType, PermissionContext? context = null, CancellationToken cancellationToken = default)
        {
            if (!await this.HasCurrentUserPermissionAsync(permissionType, context, cancellationToken))
            {
                throw new AppException(ExceptionCode.SecurityValidation);
            }
        }

        public async Task<bool> HasCurrentUserPermissionAsync(PermissionType permissionType, PermissionContext? context = null, CancellationToken cancellationToken = default)
        {
            var currentUser = await userContext.GetCurrentUserAsync(cancellationToken);

            if (currentUser.IsSystemAdmin)
            {
                return true;
            }

            switch (permissionType)
            {
                case PermissionType.System_ManageSettings when currentUser.IsSystemAdmin:
                    return true;
                case PermissionType.System_Admin when currentUser.IsSystemAdmin:
                    return true;
                case PermissionType.System_Editor:
                    return true;
                case PermissionType.System_View:
                    return true;
                default:
                    return false;
            }
        }

        public async Task<IEnumerable<PermissionType>> GetCurrentUserPermissionAsync(PermissionContext? context = null, CancellationToken cancellationToken = default)
        {
            // Can be implement in Sql server rule

            var currentUser = await userContext.GetCurrentUserAsync(cancellationToken);

            if (currentUser.IsSystemAdmin)
            {
                return Enum.GetValues<PermissionType>();
            }

            return Enumerable.Empty<PermissionType>();
        }
    }
}
