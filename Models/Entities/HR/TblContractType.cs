using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.HR;

public partial class TblContractType
{
    public string ContractTypeCode { get; set; } = null!;

    public string ContractTypeName { get; set; } = null!;

    public virtual ICollection<TblContract> TblContracts { get; set; } = new List<TblContract>();
}
