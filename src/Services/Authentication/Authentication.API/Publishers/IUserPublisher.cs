using Authentication.API.Dtos;
using System;
using System.Threading.Tasks;

namespace Authentication.API.Publishers
{
    public interface IUserPublisher
    {
        /// <summary>
        /// Publish newly created user
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Throws when request is null or has empty fields</exception>
        Task PublishNewUserAsync(PublishNewUserRequest request);
    }
}