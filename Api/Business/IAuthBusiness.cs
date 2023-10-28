using Api.Models.InputModel;
using Api.Models.ViewModels;

namespace Api.Business
{
    public interface IAuthBusiness
    {
        Task<LoggedUser?> LoginAsync(LogInUser logInUser);
        Task<string> CreateAsync(LogInUser logInUser);
    }
}
