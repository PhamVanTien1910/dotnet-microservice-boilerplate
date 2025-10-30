using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NotificationService.Options
{
    public class PortalKeySettings
    {
        public const string SectionName = "PortalKeys";
        public string Provider { get; set; } = string.Empty;
        public string Admin { get; set; } = string.Empty;
        public string Parent { get; set; } = string.Empty;
    }
}