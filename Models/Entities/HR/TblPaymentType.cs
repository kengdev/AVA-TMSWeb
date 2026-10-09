using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.HR;

public partial class TblPaymentType
{
    public string PaymentTypeCode { get; set; } = null!;

    public string PaymentTypeName { get; set; } = null!;
}
