//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SHWD.DataTranslate.WDDataProvider
{
    using System;
    using System.Collections.Generic;
    
    public partial class T_Tasks
    {
        public long TaskId { get; set; }
        public byte Status { get; set; }
        public Nullable<System.DateTime> SendTime { get; set; }
        public int CmdType { get; set; }
        public int CmdByte { get; set; }
        public int DevId { get; set; }
        public byte[] Data { get; set; }
        public short Length { get; set; }
    }
}
