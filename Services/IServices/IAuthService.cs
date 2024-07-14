using SimpleDrive.Models.Identity;

namespace SimpleDrive.Services.IServices
{
    public interface IAuthService
    {
        Task<AuthModel> RegisterAsync (RegisterModel model);
        Task<AuthModel> GetTokenAsync(TokenRequestModel model);
        Task<string> AddRoleAsync(AddRoleModel model);
    }
}
