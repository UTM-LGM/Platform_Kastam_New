using Platform_Kastam.Helpers;

namespace Platform_Kastam.Services
{
    public interface IEmailService
    {
        Task SendEmailByService(MailRequest mailrequest);
    }
}
