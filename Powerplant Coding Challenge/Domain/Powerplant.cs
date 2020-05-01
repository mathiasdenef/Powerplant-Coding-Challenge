using Domain.Enums;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Domain
{
    public class Powerplant
    {
        public string Name { get; set; }
        public PowerplantType Type { get; set; }
        public decimal Efficiency { get; set; }
        public int Pmin { get; set; }
        public int Pmax { get; set; }

        public decimal PricePerMWh { get; set; }
        public decimal PricePmin { get; set; }
        public decimal PriceRemainingLoad { get; set; }
    }
}
