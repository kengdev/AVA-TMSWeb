using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class TblYardport
{
    public string YardportCode { get; set; } = null!;

    public decimal ImportFee { get; set; }

    public decimal ExportFee { get; set; }

    public virtual ICollection<TblPortCashing> TblPortCashings { get; set; } = new List<TblPortCashing>();

    public virtual TblYardCashing? TblYardCashing { get; set; }

    public virtual TblPlace YardportCodeNavigation { get; set; } = null!;
}
