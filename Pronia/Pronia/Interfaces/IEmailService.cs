﻿namespace Pronia.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string emailTo, string subject, string body, bool isHtml = false);
    }
}
