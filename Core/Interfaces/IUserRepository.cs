using Core.DTO;
using Core.DTO.AccountingDTO;
using Core.DTO.UserDTO;
using Microsoft.AspNetCore.Http;

namespace Core.Interfaces
{
    public interface IUserRepository
    {
        Task<StringResult> Add(RegisterDTO User);
        ShowUserDTO GetUser(string id);
        void DeleteImage(string image);
        Task<StringResult> AddImage(IFormFile image, string id);
    }
}
