using Business.Interfaces;
using Domain;
using Domain.Enums;
using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Business.Services
{
    public class ProductionPlan2Service : IProductionPlanService
    {
        public ProductionPlan GetProductionPlanByPayload(Payload payload)
        {
            CalculatePricePerMWh(payload);

            CalculateWindturbinesPmax(payload);

            var powerplants = payload.Powerplants.OrderBy(x => x.PricePerMWh).ToList();
            var remainingLoad = payload.Load;

            var productionPlan = new ProductionPlan()
            {
                PowerplantDeliveries = new Collection<PowerplantDelivery>()
            };

            for (var i = 0; i < powerplants.Count; i++)
            {
                var powerplant = powerplants[i];
                if (remainingLoad <= 0)
                {
                    productionPlan.AddPowerplantDelivery(powerplant.Name, 0);
                }
                else if (remainingLoad <= powerplant.Pmin)
                {
                    productionPlan.AddPowerplantDelivery(powerplant.Name, powerplant.Pmin);
                    remainingLoad -= powerplant.Pmin;
                }
                else if (remainingLoad <= powerplant.Pmax)
                {
                    productionPlan.AddPowerplantDelivery(powerplant.Name, remainingLoad);
                    remainingLoad -= remainingLoad;
                }
                else if (remainingLoad > powerplant.Pmax)
                {
                    var nextPowerplant = GetNextPowerplant(powerplants, i);

                    if (nextPowerplant == null || ((remainingLoad - powerplant.Pmax) >= nextPowerplant.Pmin))
                    {
                        productionPlan.AddPowerplantDelivery(powerplant.Name, powerplant.Pmax);
                        remainingLoad -= powerplant.Pmax;
                        continue;
                    }

                    var notUsedPowerplants = powerplants.Where(x => !productionPlan.PowerplantDeliveries.Select(y => y.Name).Contains(x.Name)).ToList();
                    var remainingLoadToCover = remainingLoad - powerplant.Pmax;

                    // Calculate the difference to know how much can be given to next powerplant
                    var powerplantPmaxPminDiff = powerplant.Pmax - powerplant.Pmin;

                    var nextEfficientPowerplant = GetNextMostEfficientPowerplantForRemaingLoad(powerplants.Skip(i + 1).ToList(), powerplantPmaxPminDiff, remainingLoadToCover);

                    var requiredLoadForNextPowerplant = nextEfficientPowerplant.Pmin - remainingLoadToCover;

                    // Check if requiredLoadForNextPowerplant is above powerplantPmaxPminDiff, can only give away as much he can (pMax-pMin)
                    var loadPowerplant = 0;
                    if (powerplantPmaxPminDiff >= requiredLoadForNextPowerplant)
                    {
                        loadPowerplant = powerplant.Pmax - requiredLoadForNextPowerplant;
                    }
                    else
                    {
                        loadPowerplant = powerplantPmaxPminDiff;
                    }

                    productionPlan.AddPowerplantDelivery(powerplant.Name, loadPowerplant);
                    remainingLoad -= loadPowerplant;
                }
            }

            return productionPlan;
        }

        private Powerplant GetNextMostEfficientPowerplantForRemaingLoad(List<Powerplant> powerplants, int previousPowerplantPmaxPminDiff, int remainingLoad)
        {


            for (int i = 0; i < powerplants.Count; i++)
            {
                var powerplant = powerplants[i];

                if (remainingLoad < powerplant.Pmin)
                {
                    if (powerplant.Pmin <= (previousPowerplantPmaxPminDiff + remainingLoad))
                    {
                        powerplant.PriceRemainingLoad = remainingLoad * powerplant.PricePerMWh;
                    }
                    else
                    {
                        powerplant.PriceRemainingLoad = (powerplant.Pmin - previousPowerplantPmaxPminDiff + remainingLoad) * powerplant.PricePerMWh;
                    }

                    // moet hier nog wel rekening houden met de pMax, kan zijn dat die niet hoog genoeg is

                }
                else
                {
                    powerplant.PriceRemainingLoad = remainingLoad * powerplant.PricePerMWh;
                }
            }

            return powerplants.OrderBy(x => x.PriceRemainingLoad).First();
        }

        private Powerplant GetNextPowerplant(List<Powerplant> powerplants, int i)
        {
            if (i != powerplants.Count - 1)
            {
                return powerplants[i + 1];
            }
            return null;
        }

        private void CalculatePricePerMWh(Payload payload)
        {
            if (payload.Fuels == null)
            {
                throw new Exception("Fuels cannot be null");
            }

            var co2PricePerMWh = (payload.Fuels.CO2PerTon / 100) * 30;

            foreach (var powerplant in payload.Powerplants)
            {
                var fuelPrice = payload.GetFuelPriceByPowerplantType(powerplant.Type);

                switch (powerplant.Type)
                {
                    case PowerplantType.Gasfired:
                        powerplant.PricePerMWh = ((1 / powerplant.Efficiency) * fuelPrice) + co2PricePerMWh;
                        break;
                    case PowerplantType.Turbojet:
                        powerplant.PricePerMWh = (1 / powerplant.Efficiency) * fuelPrice;
                        break;
                    case PowerplantType.Windturbine:
                        // Wind has no cost
                        powerplant.PricePerMWh = 0;
                        break;
                    default:
                        break;
                }
            }
        }

        private void CalculateWindturbinesPmax(Payload payload)
        {
            var windturbinePowerplants = payload.Powerplants.Where(x => x.Type == PowerplantType.Windturbine);
            foreach (var powerplant in windturbinePowerplants)
            {
                var windPercentage = payload.GetFuelPriceByPowerplantType(PowerplantType.Windturbine);

                // Using int so it's rounded automatically
                powerplant.Pmax = decimal.ToInt32((powerplant.Pmax * windPercentage) / 100);
            }
        }



    }
}
