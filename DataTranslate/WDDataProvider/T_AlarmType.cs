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
    
    public partial class T_AlarmType
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public T_AlarmType()
        {
            this.T_Stats = new HashSet<T_Stats>();
        }
    
        public int Id { get; set; }
        public string Name { get; set; }
        public Nullable<double> DustRange { get; set; }
        public Nullable<double> DbRange { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<T_Stats> T_Stats { get; set; }
    }
}
