using Authentication.API.Dtos;
using System;
using System.Threading.Tasks;

namespace Authentication.API.Services
{
    public interface IAuthService
    {
        /// <summary>
        /// Register new valid user
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Throws when request properity is empty</exception>
        /// <exception cref="ArgumentNullException">Throws when request is null</exception>
        Task<RegisterResult> RegisterAsync(RegisterRequest request);
    }
}