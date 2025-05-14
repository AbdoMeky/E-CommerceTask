using Core.DTO;
using Core.DTO.AccountingDTO;
using Core.DTO.UserDTO;
using Core.Entities;
using Core.Interfaces;
using Core.Utilities;
using IF.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace IF.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly string _storagePath;
        private readonly string _backupDirPath;
        public UserRepository(AppDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _storagePath = Path.Combine(_webHostEnvironment.WebRootPath, "UploadedImagesForUserImage");
            _backupDirPath = Path.Combine(_webHostEnvironment.WebRootPath, "BackupForUserImage");
        }
        public async Task<StringResult> Add(RegisterDTO user)
        {
            if (!Directory.Exists(_storagePath))
            {
                Directory.CreateDirectory(_storagePath);
            }
            var filePath = AddImageHelper.CheckImagePath(user.Image,_storagePath);
            if (!string.IsNullOrEmpty(filePath.Message))
            {
                return new StringResult() { Message = filePath.Message };
            }
            var newUser = new ApplicationUser
            {
                ProfilePictureUrl = filePath.Id,
                Name = user.Name,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                UserName = user.UserName,
            };
            _context.Users.Add(newUser);
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    await _context.SaveChangesAsync();
                    if (!string.IsNullOrEmpty(filePath.Id))
                    {
                        using (var stream = new FileStream(filePath.Id, FileMode.Create))
                        {
                            await user.Image.CopyToAsync(stream);
                        }
                    }
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    return new StringResult() { Message = ex.Message };
                }
            }
            return new StringResult() { Id = newUser.Id };
        }
        public ShowUserDTO GetUser(string id)
        {
            var user = _context.Users.Find(id);
            if (user is null)
            {
                return null;
            }
            return new ShowUserDTO
            {
                Name = user.Name,
                ProfilePictureUrl = user.ProfilePictureUrl,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
            };
        }
        public async Task<StringResult> AddImage(IFormFile image, string id)
        {
            if (!Directory.Exists(_storagePath))
            {
                Directory.CreateDirectory(_storagePath);
            }
            var filePath = AddImageHelper.CheckImagePath(image,_storagePath);
            if (!string.IsNullOrEmpty(filePath.Message))
            {
                return new StringResult() { Message = filePath.Message };
            }
            var user = _context.Users.Find(id);
            user.ProfilePictureUrl = filePath.Id;
            try
            {

                await _context.SaveChangesAsync();
                if (!string.IsNullOrEmpty(filePath.Id))
                {
                    using (var stream = new FileStream(filePath.Id, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }
                }
            }
            catch (Exception ex)
            {
                return new StringResult() { Message = ex.Message };
            }
            return new StringResult() { Id = filePath.Id };
        }
        public void DeleteImage(string oldImagePath)
        {
            if (File.Exists(oldImagePath))
            {
                File.Delete(oldImagePath);
            }
        }
    }
}
