using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CarbonWise.API.Controller.CarbonFootprints
{
    public class YearCarbonFootprintRequest
    {
        [Required]
        [Range(2000, 2100)]
        public int Year { get; set; }

        public decimal? ElectricityFactor { get; set; }

        public decimal? ShuttleBusFactor { get; set; }

        public decimal? CarFactor { get; set; }

        public decimal? MotorcycleFactor { get; set; }
    }

    public class PeriodCarbonFootprintRequest
    {
        [DefaultValue("2000-01-01T00:00:00Z")]
        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        [DefaultValue("2025-01-01T00:00:00Z")]
        public DateTime EndDate { get; set; }

        public decimal? ElectricityFactor { get; set; }

        public decimal? ShuttleBusFactor { get; set; }

        public decimal? CarFactor { get; set; }

        public decimal? MotorcycleFactor { get; set; }
    }
}
