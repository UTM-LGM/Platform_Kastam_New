using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Renci.SshNet.Common;
using Renci.SshNet;
using System.Net.Sockets;
using Platform_Kastam.Services;
using Platform_Kastam.Helpers;

namespace Platform_Kastam.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IntegrationKastamController : ControllerBase
    {
        private readonly IEmailService emailService;
        public IntegrationKastamController(IEmailService emailService)
        {
            this.emailService = emailService;
        }

        [HttpGet("test-connection")]
        public IActionResult TestConnection()
        {
            try
            {
                using var client = new SftpClient("150.242.182.49", 22, "lgm01", "lgm01");

                // Timeouts
                client.ConnectionInfo.Timeout = TimeSpan.FromSeconds(60);
                client.OperationTimeout = TimeSpan.FromSeconds(60);        // read/write timeout
                client.KeepAliveInterval = TimeSpan.FromSeconds(15);       // prevent idle drop


                client.Connect();
                bool isConnected = client.IsConnected;
                client.Disconnect();

                return Ok(new { Success = isConnected, Message = "Connection successful" });
            }
            catch (SshAuthenticationException ex)
            {
                return BadRequest(new { Success = false, Error = "Authentication failed: " + ex.Message });
            }
            catch (SshConnectionException ex)
            {
                return BadRequest(new { Success = false, Error = "Connection failed: " + ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Error = ex.Message });
            }
        }

        [HttpPost("SendMail")]
        public async Task<IActionResult> SendMail2()
        {
            try
            {
                MailRequest mailrequest = new MailRequest();
                mailrequest.ToEmail = "syahmirahim99@gmail.com";
                mailrequest.Subject = "[Notifikasi Data Kastam] - Fail Kastam Berjaya Dimuatnaik";
                mailrequest.Body = this.GetHtmlContent();
                await emailService.SendEmailByService(mailrequest);
                return Ok();
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        private string GetHtmlContent()
        {
            string response = @"
                                <div style='font-family: Arial, Helvetica, sans-serif; font-size: 14px; color: #333;'>
                                    <p>Salam Sejahtera,</p>

                                    <p>
                                        Dimaklumkan bahawa <b>fail Data Kastam</b> telah
                                        <b>berjaya dimuat naik</b> ke dalam sistem.
                                    </p>

                                    <table style='margin-top: 15px; border-collapse: collapse;'>
                                        <tr>
                                            <td style='padding: 6px 10px; font-weight: bold;'>Status</td>
                                            <td style='padding: 6px 10px;'>Berjaya</td>
                                        </tr>
                                        <tr>
                                            <td style='padding: 6px 10px; font-weight: bold;'>Tarikh & Masa</td>
                                            <td style='padding: 6px 10px;'>" + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + @"</td>
                                        </tr>
                                    </table>

                                    <p style='margin-top: 20px;'>
                                        Tiada tindakan lanjut diperlukan.
                                    </p>

                                    <p>
                                        Sekian, terima kasih.
                                    </p>

                                    <br />
                                    <p style='font-size: 12px; color: #777;'>
                                        ~ Sistem Integrasi Data Kastam  ~
                                    </p>
                                </div>";

            return response;
        }

        [HttpPost("DownloadFileKastam")]
        public IActionResult DownloadFileKastam()
        {
            // --- TARGET SETTINGS (Where the files go) ---
            string localBaseDir = @"C:\RootFolderKastam"; // Change this to your local path

            // --- REMOTE SETTINGS (Where the files come from) ---
            string host = "192.168.1.100";  // Remote Server IP
            string user = "lgm01";           // SSH Username
            string pass = "lgm01";   // SSH Password
            string remoteDir = "/";         // Remote Root Folder

            // 1. Setup Dates
            DateTime yesterday = DateTime.Now.AddDays(-1).Date;
            string folderName = yesterday.ToString("yyyy-MM-dd");
            string finalLocalPath = Path.Combine(localBaseDir, folderName);

            int downloadedCount = 0;

            try
            {
                using (var client = new SftpClient(host, user, pass))
                {
                    client.Connect();

                    // 2. Filter for .txt files modified yesterday
                    var files = client.ListDirectory(remoteDir).Where(f =>
                        !f.IsDirectory &&
                        f.Name.EndsWith(".txt", StringComparison.OrdinalIgnoreCase) &&
                        f.LastWriteTime.Date == yesterday);

                    if (files.Any())
                    {
                        // Create target folder if it doesn't exist
                        if (!Directory.Exists(finalLocalPath))
                            Directory.CreateDirectory(finalLocalPath);

                        foreach (var file in files)
                        {
                            string destination = Path.Combine(finalLocalPath, file.Name);
                            using (var fs = System.IO.File.Create(destination))
                            {
                                client.DownloadFile(file.FullName, fs);
                            }
                            downloadedCount++;
                        }
                    }

                    client.Disconnect();
                }

                return Ok(new
                {
                    Message = "Download Complete",
                    FilesFound = downloadedCount,
                    SavedAt = finalLocalPath
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Connection failed: {ex.Message}");
            }
        }

    }
}
