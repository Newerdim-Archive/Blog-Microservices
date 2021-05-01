using Authentication.API.Dtos;
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
        Task PublishNewUser(PublishNewUserRequest request);
    }
}
