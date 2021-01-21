using System;
using System.Collections.Generic;
using System.Text;

namespace SecureCommunication.Model
{
    public enum BackStatus
    {
        NOOP=0,
        WAITNEXT,
        ABANDONED,
    }
    public class ProtocolBackModel
    {
        public BackStatus Status { get; set; }
        public byte[] Array { get; set; }
    }
}
