using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class TblReceiptType
{
    public string ReceiptTypeCode { get; set; } = null!;

    public string ReceiptTypeName { get; set; } = null!;

    public virtual ICollection<TblReceiptOther> TblReceiptOthers { get; set; } = new List<TblReceiptOther>();
}
