using System;

namespace GoBangladesh.Application.ViewModels
{
    public class PayloadResponse
    {
        public bool IsSuccess { get; set; }
        public dynamic Content { get; set; }
        public string TimeStamp { get; set; }
        public string PayloadType { get; set; }
        public string Message { get; set; }
        public PayloadResponse()
        {
            TimeStamp = DateTime.UtcNow.ToString("[dd/MM/yyyy#HHmmss]");
        }
    }
}
