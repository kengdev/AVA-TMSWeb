using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TMSWeb.Models;
using TMSWeb.Models.Entities;

namespace TMSWeb.Models.ViewModels;
public class FusionBarChart
{
    public FCBarAttribute chart { get; set; }
    public FCData[] data { get; set; }
}

public class FusionMSLine
{
    public FCMSLineAttribute chart { get; set; }
    public FCCategory categories { get; set; }
    public FCDataset[] dataset { get; set; }
}

public class FCBarAttribute
{
    public string plotgradientcolor { get; set; }
    public string showalternatehgridcolor { get; set; }
    public string showShadow { get; set; }
    public string showplotborder { get; set; }
    public string showvalues { get; set; }
    public string theme { get; set; }
    public string divlinecolor { get; set; }
    public string numberSuffix { get; set; }
    public string palettecolors { get; set; }
    public string toolTipBorderColor { get; set; }
    public string toolTipBgColor { get; set; }
    public string toolTipColor { get; set; }
    public string showcanvasborder { get; set; }
    public string canvasborderalpha { get; set; }
    public string anchorRadius { get; set; }
    public string anchorBorderThickness { get; set; }
    public string yaxisvaluespadding { get; set; }
    public string showborder { get; set; }
    public string outcnvbasefontsize { get; set; }
    public string showXAxisLine { get; set; }
}
public class FCData
{
    public string label { get; set; }
    public string value { get; set; }
}

public class FCMSLineAttribute
{
    public string theme { get; set; }
    public string caption { get; set; }
    public string subCaption { get; set; }
    public string xAxisName { get; set; }
    public string yAxisName { get; set; }
    public string showXAxisLine { get; set; }
    public string xAxisLineColor { get; set; }
}
public class FCCategory
{
    public CategoryValue[] category { get; set; }
}
public class CategoryValue
{
    public string label { get; set; }
}
public class CategoryAttribute
{
    public string[] label { get; set; }
}
public class FCDataset
{
    public string seriesname { get; set; }
    public FCDatasetValue[] data { get; set; }
}
public class FCDatasetValue
{
    public string value { get; set; }
}