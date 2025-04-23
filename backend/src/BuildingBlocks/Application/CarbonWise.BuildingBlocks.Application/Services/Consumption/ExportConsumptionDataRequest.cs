using System;
using System.ComponentModel.DataAnnotations;

namespace CarbonWise.API.Models.Consumption
{
    public class ExportConsumptionDataRequest
    {
        [Required]
        [RegularExpression("^(Water|Electric|NaturalGas|Paper)$", ErrorMessage = "ConsumptionType must be one of: Water, Electric, NaturalGas, Paper")]
        public string ConsumptionType { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public bool IncludeGraphs { get; set; } = false;
    }
}