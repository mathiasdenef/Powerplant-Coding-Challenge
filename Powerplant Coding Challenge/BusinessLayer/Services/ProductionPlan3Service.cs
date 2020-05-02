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
    public class ProductionPlan3Service : IProductionPlanService
    {
        public ProductionPlan GetProductionPlanByPayload(Payload payload)
        {
            //// CalculatePricePerMWh for every powerplant
            //CalculatePricePerMWh(payload);
            //// Calculate Pmin and Pmax for winturbines
            //CalculateWindturbinesPmax(payload);
            //// Create a ProductionPlan with all PowerplantDeliveries p = Pmax
            //var productionPlan = new ProductionPlan();
            //for (var i = 0; i < payload.Powerplants.Count; i++)
            //{
            //    var powerplant = payload.Powerplants[i];
            //    productionPlan.AddPowerplantDelivery(powerplant.Name, powerplant.Pmax);
            //}
            //// Lower the power of PowerplantDeliveries, least efficient ones first, untill the total power equals necessary load

            //LowerPowerIfExcessiveLoad(productionPlan, payload);


            return TestMethod(payload);
        }

        private void LowerPowerIfExcessiveLoad(ProductionPlan productionPlan, Payload payload)
        {
            var runningPowerplantDeliveries = productionPlan.PowerplantDeliveries.Where(x => x.P != 0).ToList();
            var totalLoadRunningPowerplantDeliveries = runningPowerplantDeliveries.Sum(x => x.P);
            var excessiveLoad = totalLoadRunningPowerplantDeliveries - payload.Load;

            // There is no excessive load, everything fits perfectly so nothing needs to change
            if (excessiveLoad == 0) return;
            // Excessive load is negative, meaning available powerplants couldn't provide enough power
            if (excessiveLoad < 0)
            {
                // Maybe throw exception here, no idea what business requirement is
                return;
            }
            // There is excessive load, lower power from least efficient powerplants (with pMin in mind)
            if (excessiveLoad > 0)
            {

                // Calculate for every running powerplant what pricePerMWh keeping in mind excessive load an Pmin

                // Calculate for every running powerplant that could 

                // Get the least efficient powerplant and 

                var possiblePowerplantDeliveriesToLower = runningPowerplantDeliveries.ToList();

                // Keep looping untill excessive load = 0 or possible powerplants to lower = 0
                while (excessiveLoad > 0 || possiblePowerplantDeliveriesToLower.Count == 0)
                {
                    // Recalculate the totalLoadRunningPowerplantDeliveries
                    runningPowerplantDeliveries = productionPlan.PowerplantDeliveries.Where(x => x.P != 0).ToList();
                    totalLoadRunningPowerplantDeliveries = runningPowerplantDeliveries.Sum(x => x.P);



                    // Check again what how many powerplants can be lowered
                    // All powerplants that are still running
                    // All powerplants that could be shutdown if Pmin can be covered by other running powerplants and if it is cost efficient

                    possiblePowerplantDeliveriesToLower = productionPlan.PowerplantDeliveries.Where(x => x.P != 0).ToList();
                    possiblePowerplantDeliveriesToLower.Where(powerplantDelivery =>
                        {
                            if (excessiveLoad == 0)
                            {

                            }
                            return true;
                        });
                }

                var powerplants = payload.Powerplants.OrderByDescending(x => x.PricePerMWh).ToList();

                for (int i = 0; i < productionPlan.PowerplantDeliveries.Count; i++)
                {
                    var powerplantDelivery = productionPlan.PowerplantDeliveries[i];

                    var excessiveRemainingLoad = excessiveLoad - powerplantDelivery.P;

                    if (excessiveRemainingLoad >= 0)
                    {
                        excessiveLoad -= powerplantDelivery.P;
                        powerplantDelivery.P = 0;
                    }
                    // If excessiveRemainingLoad is below 0, I need to calculate PricePerMWh keeping in mind Pmin and efficient powerplants power they can spare (Pmax - Pmin) 
                    else if (excessiveRemainingLoad < 0)
                    {
                        // Get powerplants that are still running, so could be lowered/shutdowned
                    }

                }
            }

        }

        private ProductionPlan TestMethod(Payload payload)
        {
            // Calculate PricePerMWh
            CalculatePricePerMWh(payload);
            // Calculate Pmax for winturbines
            CalculateWindturbinesPmax(payload);

            // Order all powerplants by PricePerMWh
            var powerplantsOrderedByCost = payload.Powerplants.OrderBy(x => x.PricePerMWh).ToList();
            var startedPowerplants = new List<Powerplant>();

            // Temporary remainingLoad for checking which powerplant needs to get started
            var remainingLoad = payload.Load;

            // Startup powerplants (p = Pmin) as many as needed
            var productionPlan = new ProductionPlan();

            for (var i = 0; i < powerplantsOrderedByCost.Count; i++)
            {
                var powerplant = powerplantsOrderedByCost[i];
                // If remaining load is covered
                if (remainingLoad <= 0)
                {
                    productionPlan.AddPowerplantDelivery(powerplant.Name, 0);
                }
                // If powerplant can NOT cover remaining load, because Pmin is too high
                else if (remainingLoad < powerplant.Pmin)
                {
                    // Make a list of not yet started powerplants
                    var notStartedPowerplants = powerplantsOrderedByCost.Where(x => !startedPowerplants.Contains(x)).ToList();
                    // Calculate the price for notStartedPowerplants
                    var mostEfficientPowerplant = GetMostEfficientRemainingPowerplantForRemainingLoad(remainingLoad, notStartedPowerplants, startedPowerplants);
                    productionPlan.AddPowerplantDelivery(mostEfficientPowerplant.Name, mostEfficientPowerplant.Pmin);
                    startedPowerplants.Add(mostEfficientPowerplant);
                    remainingLoad -= mostEfficientPowerplant.Pmax;
                    // Move the cheapest option to the index of current loop, so current powerplant will be added in next loop
                }
                // If powerplant can NOT cover remaining load, because Pmax is too low
                else if (remainingLoad > powerplant.Pmax)
                {
                    productionPlan.AddPowerplantDelivery(powerplant.Name, powerplant.Pmin);
                    startedPowerplants.Add(powerplant);
                    remainingLoad -= powerplant.Pmax;
                }
                // If powerplant can cover remaining load 
                else if (remainingLoad <= powerplant.Pmax)
                {
                    productionPlan.AddPowerplantDelivery(powerplant.Name, powerplant.Pmin);
                    startedPowerplants.Add(powerplant);
                    remainingLoad -= remainingLoad;
                }
            }



            // When powerplant is getting started and you know spreading needs to happen (remaining load < Pmin) => calculate for all the remaining powerplants what it would cost them
            // For calculation: If remaining powerplant has a Pmin, check how much can be used from previous powerplants and calculate that cost difference


            // Spread the remaining load (load - sum of Pmin) efficient across all powerplants by checking what the cost is PerMWh
            SpreadLoadOverStartedPowerplants(productionPlan, startedPowerplants, payload.Load);

            return productionPlan;
        }

        private void SpreadLoadOverStartedPowerplants(ProductionPlan productionPlan, List<Powerplant> startedPowerplants, int load)
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

                powerplant.Pmax = decimal.ToInt32((powerplant.Pmax * windPercentage) / 100);
            }
        }



    }
}
