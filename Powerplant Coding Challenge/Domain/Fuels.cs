using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Domain
{
    public class Fuels
    {
        [JsonPropertyName("gas(euro/MWh)")]
        public decimal GasPricePerMWh { get; set; }
        [JsonPropertyName("kerosine(euro/MWh)")]
        public decimal KerosinePricePerMWh { get; set; }
        [JsonPropertyName("co2(euro/ton)")]
        public decimal CO2PerTon { get; set; }
        [JsonPropertyName("wind(%)")]
        public int WindPercentage { get; set; }
    }
}
