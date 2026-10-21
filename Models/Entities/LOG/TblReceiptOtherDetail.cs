using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class TblReceiptOtherDetail
{
    public int Id { get; set; }

    public string? ReceiptOtherName { get; set; }

    public string? Description { get; set; }

    public string? ContainerSize { get; set; }

    public int Quantity { get; set; }

    public decimal Price { get; set; }

    public decimal Total { get; set; }

    public string ReceiptOtherCode { get; set; } = null!;

    public virtual TblReceiptOther ReceiptOtherCodeNavigation { get; set; } = null!;
}
