using System;
using System.Collections.Generic;
using System.Text;

namespace Agenciapp.Service.IMarketing.Models
{
    public class SendCampaingModel
    {
        public SendCampaingModel(List<string> phonesNumbers, string messages)
        {
            PhoneNumbers = phonesNumbers;
            Message = messages;
        }
        public SendCampaingModel(List<string> phonesNumbers, string messages, List<Uri> files)
        {
            PhoneNumbers = phonesNumbers;
            Message = messages;
            Files = files;
        }

        public List<string> PhoneNumbers { get; set; }
        public string Message { get; set; }
        public List<Uri> Files { get; set; }
    }
}
