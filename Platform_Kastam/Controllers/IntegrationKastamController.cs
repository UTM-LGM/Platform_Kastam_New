using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Renci.SshNet.Common;
using Renci.SshNet;
using System.Net.Sockets;

namespace Platform_Kastam.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IntegrationKastamController : ControllerBase
    {
        // --- HARDCODED CREDENTIALS ---
        private readonly string _host = "123.123.123.123";
        private readonly int _port = 22;
        private readonly string _username = "your_ssh_user";
        private readonly string _password = "your_password";


        public IntegrationKastamController()
        {
            
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


    }
}
