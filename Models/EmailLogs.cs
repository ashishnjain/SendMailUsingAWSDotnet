using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SendMailforAWSorSMTP.Models
{
    public class EmailLogs
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Campaign { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public string MessageID { get; set; }
        public Nullable<bool> isDelivered { get; set; }
        public DateTime? sentOn { get; set; }
        public DateTime? deliveredOn { get; set; }
        public string replyTo { get; set; }
    }
}
