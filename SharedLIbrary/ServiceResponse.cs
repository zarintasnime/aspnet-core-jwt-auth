using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLIbrary
{
    public class ServiceResponse
    {
        public record GeneralResponse(bool Flag, string Message);
        public record LoginResponse(bool Flag, string Message, string Token);
        public record UserSession(string? Id, string? Name, string? Email, string? Role);
    }
}
