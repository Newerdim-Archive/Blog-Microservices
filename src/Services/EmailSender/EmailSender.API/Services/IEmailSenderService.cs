﻿using EmailSender.API.Dtos;
using System;
using System.Threading.Tasks;

namespace EmailSender.API.Services
{
    /// <summary>
    /// Service that sends emails
    /// </summary>
    public interface IEmailSenderService
    {
        /// <summary>
        /// Send async email.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Throws when message is null or email is invalid.</exception>
        public Task SendAsync(SendRequest request);
    }
}