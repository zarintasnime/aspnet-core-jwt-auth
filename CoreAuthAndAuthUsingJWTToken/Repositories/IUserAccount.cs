using SharedLIbrary;
using static SharedLIbrary.ServiceResponse;

namespace CoreAuthAndAuthUsingJWTToken.Repositories
{
    public interface IUserAccount
    {
        Task<GeneralResponse> CreateAccount(UserDTO userDTO);
        Task<LoginResponse> LoginAccount(LoginDTO loginDTO);
    }
}
