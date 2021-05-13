using Authentication.API.Dtos;
using System;
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
        /// <exception cref="ArgumentException">Throws when request is null or has empty fields</exception>
        Task PublishEmailConfirmationAsync(PublishEmailConfirmationRequest request);
    }
}