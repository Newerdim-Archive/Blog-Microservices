using Authentication.API.Dtos;
using System.Threading.Tasks;

namespace Authentication.API.Publishers
{
    public interface IEmailPublisher
    {
        /// <summary>
        /// Publish email confirmation
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task PublishEmailConfirmation(PublishEmailConfirmationRequest request);
    }
}
