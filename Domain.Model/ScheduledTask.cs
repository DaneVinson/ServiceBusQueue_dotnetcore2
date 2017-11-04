using System;

namespace Domain.Model
{
    public class ScheduledTask
    {
        public string Description { get; set; }
        public int ShardId { get; set; }
        public string TaskId { get; set; }

        public override string ToString()
        {
            return $"{TaskId}: {Description} on shard {ShardId}";
        }
    }
}
