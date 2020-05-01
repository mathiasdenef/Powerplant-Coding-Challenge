using Domain.Enums;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Domain
{
    public class Payload
    {
        public int Load { get; set; }
        public Fuels Fuels { get; set; }
        public IList<Powerplant> Powerplants { get; set; }

        public decimal GetFuelPriceByPowerplantType(PowerplantType type)
        {
            switch (type)
            {
                case PowerplantType.Gasfired:
                    return Fuels.GasPricePerMWh;
                case PowerplantType.Turbojet:
                    return Fuels.KerosinePricePerMWh;
                case PowerplantType.Windturbine:
                    return Fuels.WindPercentage;
                default:
                    throw new Exception("No correct PowerplantType used.");
            }
        }
    }
}
