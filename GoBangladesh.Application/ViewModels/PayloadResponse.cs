using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            TimeStamp = DateTime.Now.ToString("[dd/MM/yyyy#HHmmss]");
        }
    }
}
