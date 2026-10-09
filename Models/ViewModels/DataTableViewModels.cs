using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TMSWeb.Models;
public class jQueryDataTableParamModel
{
    /// Request sequence number sent by DataTable,
    /// same value must be returned in response
    public string sEcho { get; set; }

    /// Text used for filtering
    public string sSearch { get; set; }

    /// Number of records that should be shown in table
    public int iDisplayLength { get; set; }

    /// First record that should be shown(used for paging)
    public int iDisplayStart { get; set; }

    /// Number of columns in table
    public int iColumns { get; set; }

    /// Number of columns that are used in sorting
    public int iSortingCols { get; set; }

    /// First sort column numeric index, possible to have 
    /// _1,_2 etc for multi column sorting
    public int iSortCol_0 { get; set; }

    /// Sort direction of the first sorted column, asc or desc
    public string sSortDir_0 { get; set; }

    /// Comma separated list of column names
    public string sColumns { get; set; }
}

public class avaDataTableParamModel
{
    private DateTime filterDateEnd;

    public string filterDate { get; set; }
    public DateTime filterDateRangeStart { get; set; }
    public DateTime filterDateRangeEnd
    {
        get { return filterDateEnd; }
        set { filterDateEnd = value.AddDays(1).AddSeconds(-1); }
    }
    public string filterCode { get; set; }
    public string filterInvoice { get; set; }
    public string filterCustomer { get; set; }
    public string filterContainerSize { get; set; }
    public string filterMission { get; set; }
    public string filterContainerNo { get; set; }
    public string filterBookingNo { get; set; }
    public string filterLicense { get; set; }
    public string filterTruck { get; set; }
    public string filterTrailer { get; set; }
    public string filterSupplier { get; set; }
    public string filterZone { get; set; }
    public string filterTruckGroup { get; set; }
    public string filterPhone { get; set; }
    public string filterNameTH { get; set; }
    public string filterNameEN { get; set; }
    public string filterFuel { get; set; }
    public string filterBrand { get; set; }
    public string filterServiceType { get; set; }
    public string filterMaintenanceType { get; set; }
    public bool? filterStatus { get; set; }
    public string filterStatusCode { get; set; }
    public string filterDept { get; set; }
    public string month { get; set; }
    public string year { get; set; }
}