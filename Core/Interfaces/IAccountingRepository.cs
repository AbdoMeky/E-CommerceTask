using Core.DTO.AccountingDTO;

namespace Core.Interfaces
{
    public interface IAccountingRepository
    {
        Task<AuthResultDTO> LogIn(LogInDTO logInDTO);
        Task<AuthResultDTO> RegisteUser(RegisterDTO UserDTO);
        Task<AuthResultDTO> CheckRefreshTokenAndRevoke(string token);
        Task<bool> RevokeRefreshToken(string token);
    }
}
