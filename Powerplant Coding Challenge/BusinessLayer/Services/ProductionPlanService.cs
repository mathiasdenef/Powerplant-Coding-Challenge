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
    public class ProductionPlanService : IProductionPlanService
    {
        public ProductionPlan GetProductionPlanForPayload(Payload payload)
        {
            ErrorHandling();

            var productionPlan = new ProductionPlan();

            CalculatePowerplantPricePerMWh(payload);

            CalculateWindturbinesPmax(payload);

            var requiredPowerplants = GetRequiredPowerplantsForPayload(payload);

            AddPowerplantsToProductionPlan(productionPlan, payload, requiredPowerplants);

            DivideLoadEfficientAcrossRequiredPowerplants(productionPlan, requiredPowerplants, payload.Load);

            // Order by power descending
            productionPlan.PowerplantDeliveries = productionPlan.PowerplantDeliveries.OrderByDescending(x => x.P).ToList();

            return productionPlan;
        }

        private List<Powerplant> GetRequiredPowerplantsForPayload(Payload payload)
        {
            // Order all powerplants by PricePerMWh
            var powerplants = payload.Powerplants.OrderBy(x => x.PricePerMWh).ToList();

            // Remaining load for calculating which powerplant is required
            var remainingLoad = payload.Load;

            // List used for return value
            var requiredPowerplants = new List<Powerplant>();

            // Keep looping untill all powerplants have been used
            while (powerplants.Count != 0)
            {
                var powerplant = powerplants.First();

                if (remainingLoad <= 0)
                {
                    powerplants.Remove(powerplant);
                }
                // If powerplant can NOT cover remaining load, because Pmin is too high
                else if (remainingLoad < powerplant.Pmin)
                {
                    var mostEfficientPowerplant = GetMostEfficientRemainingPowerplantForRemainingLoad(remainingLoad, powerplants, requiredPowerplants);
                    requiredPowerplants.Add(mostEfficientPowerplant);
                    remainingLoad -= mostEfficientPowerplant.Pmax;
                    powerplants.Remove(mostEfficientPowerplant);
                }
                // If powerplant can NOT cover remaining load, because Pmax is too low
                else if (remainingLoad > powerplant.Pmax)
                {
                    requiredPowerplants.Add(powerplant);
                    remainingLoad -= powerplant.Pmax;
                    powerplants.Remove(powerplant);
                }
                // If powerplant can cover remaining load 
                else if (remainingLoad <= powerplant.Pmax)
                {
                    requiredPowerplants.Add(powerplant);
                    remainingLoad -= remainingLoad;
                    powerplants.Remove(powerplant);
                }
            }
            return requiredPowerplants;
        }

        private void AddPowerplantsToProductionPlan(ProductionPlan productionPlan, Payload payload, List<Powerplant> requiredPowerplants)
        {
            // First we add the require
            for (var i = 0; i < payload.Powerplants.Count; i++)
            {
                var powerplant = payload.Powerplants[i];

                // If powerplant is in required powerplants list, add with Pmin value
                if (requiredPowerplants.Contains(powerplant))
                {
                    productionPlan.AddPowerplantDelivery(powerplant.Name, powerplant.Pmin);

                }
                else
                {
                    productionPlan.AddPowerplantDelivery(powerplant.Name, 0);
                }
            }
        }

        private void DivideLoadEfficientAcrossRequiredPowerplants(ProductionPlan productionPlan, List<Powerplant> startedPowerplants, int load)
        {
            var powerplantDeliveries = productionPlan.PowerplantDeliveries.ToList();

            var remainingLoad = load - powerplantDeliveries.Sum(x => x.P);

            // If remaining load is 0 or negative, no spreading is necessary
            if (remainingLoad <= 0) return;

            for (var i = 0; i < startedPowerplants.Count; i++)
            {
                var powerplant = startedPowerplants[i];
                if (remainingLoad > powerplant.Pmax - powerplant.Pmin)
                {
                    powerplantDeliveries.Find(x => x.Name == powerplant.Name).P = powerplant.Pmax;
                    remainingLoad -= (powerplant.Pmax - powerplant.Pmin);
                }
                else
                {
                    powerplantDeliveries.Find(x => x.Name == powerplant.Name).P = powerplant.Pmin + remainingLoad;
                    remainingLoad -= remainingLoad;
                }
            }
        }

        private Powerplant GetMostEfficientRemainingPowerplantForRemainingLoad(int remainingLoad, List<Powerplant> notStartedPowerplants, List<Powerplant> startedPowerplants)
        {
            // If Pmin > 0 check if used powerplants can be lowered and what it would cost and where it would be taken from
            // Compare the prices and take the cheapest option
            // Add the cheapest option with p = Pmin

            // Move the cheapest option to the index of current loop, so current powerplant will be added in next loop

            Powerplant mostEfficientPowerplant = notStartedPowerplants[0]; // By default the next in line is cheapestOption, necessary for comparison
            for (var i = 0; i < notStartedPowerplants.Count; i++)
            {
                var powerplant = notStartedPowerplants[i];
                // If Pmin = 0, calculating price for remaining load is easy
                if (powerplant.Pmin == 0)
                {
                    powerplant.PriceRemainingLoad = remainingLoad * powerplant.PricePerMWh;
                }
                // If Pmin > 0 check if used powerplants can be lowered and what it would cost and where it would be taken from 
                else
                {
                    var missingLoad = GetMissingLoad(powerplant, remainingLoad);
                    // If not missing load, price is just remaining load * price per MWh
                    if (missingLoad == 0)
                    {
                        powerplant.PriceRemainingLoad = remainingLoad * powerplant.PricePerMWh;
                    }
                    // If there is missing load calc the cost for Pmin - costs you are saving from where you take power
                    else
                    {
                        var pricePmin = powerplant.Pmin * powerplant.PricePerMWh;
                        var startedPowerplantsOrderDescByPricePerMWh = startedPowerplants.OrderByDescending(x => x.PricePerMWh).ToList();
                        var loadNeededFromStartedPowerplants = powerplant.Pmin - remainingLoad;
                        var totalCostForMissingLoad = pricePmin;

                        for (var j = 0; j < startedPowerplantsOrderDescByPricePerMWh.Count; j++)
                        {
                            var startedPowerplant = startedPowerplantsOrderDescByPricePerMWh[j];
                            var differencePmaxPmin = startedPowerplant.Pmax - startedPowerplant.Pmin;
                            // Can cover it
                            if (loadNeededFromStartedPowerplants <= differencePmaxPmin)
                            {
                                totalCostForMissingLoad -= loadNeededFromStartedPowerplants * startedPowerplant.PricePerMWh;
                                break; // Calculated the totalCost so don't need to loop further
                            }
                            else
                            {
                                totalCostForMissingLoad -= differencePmaxPmin * startedPowerplant.PricePerMWh;
                                loadNeededFromStartedPowerplants -= differencePmaxPmin;
                            }
                        }
                        powerplant.PriceRemainingLoad = totalCostForMissingLoad;
                    }
                }
            }

            return notStartedPowerplants.OrderBy(x => x.PriceRemainingLoad).First();
        }



        private int GetMissingLoad(Powerplant powerplant, int remainingLoad)
        {
            int missingLoad;
            // If the difference is positive number, calc missing load
            if ((powerplant.Pmin - remainingLoad) >= 0)
            {
                missingLoad = powerplant.Pmin - remainingLoad;
            }
            // remaining load can be covered
            else
            {
                missingLoad = 0;
            }
            return missingLoad;
        }

        private Powerplant GetNextPowerplant(List<Powerplant> powerplants, int i)
        {
            if (i + 1 != powerplants.Count)
            {
                return powerplants[i + 1];
            }
            return null;
        }

        private void CalculatePowerplantPricePerMWh(Payload payload)
        {
            // 1MWh = 0.30 Ton CO2
            var co2PricePerMWh = (payload.Fuels.CO2PerTon / 100) * 30;

            foreach (var powerplant in payload.Powerplants)
            {
                var fuelPrice = payload.GetFuelPriceByPowerplantType(powerplant.Type);

                switch (powerplant.Type)
                {
                    case PowerplantType.Gasfired:
                        // Also taking CO2 emission cost into account for gas-fired powerplants
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

                // Adjusting Pmax according to provided windPercentage
                powerplant.Pmax = decimal.ToInt32((powerplant.Pmax * windPercentage) / 100);
            }
        }

        private void ErrorHandling()
        {

        }

    }
}
