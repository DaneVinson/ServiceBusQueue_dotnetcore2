using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Model
{
    public class JobStartOptions
    {
        public string EndpointAddress { get; set; }
        public bool IsReport { get; set; }
        public string JobId { get; set; }
        public int ShardId { get; set; }
    }
}
