namespace AuthService.Api.Infrastructure.Interface
{
    public interface IMenuPermissionRepository
    {
        Task<string[]> GetPermissionList(Guid? UserId);
    }
}
