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
        /// <summary>
        /// Get the production for specified payload
        /// </summary>
        /// <param name="payload"></param>
        /// <returns>A productionplan containing the powerplant deliveries for specified payload</returns>
        public ProductionPlan GetProductionPlanForPayload(Payload payload)
        {
            ErrorHandling(payload);

            var productionPlan = new ProductionPlan();

            CalculatePowerplantPricePerMWh(payload);

            CalculateWindturbinesPmax(payload);

            var requiredPowerplants = GetRequiredPowerplantsForPayload(payload);

            AddPowerplantsToProductionPlan(productionPlan, payload, requiredPowerplants);

            DivideLoadEfficientBetweenRequiredPowerplants(productionPlan, requiredPowerplants, payload.Load);

            // Order by power descending
            productionPlan.PowerplantDeliveries = productionPlan.PowerplantDeliveries.OrderByDescending(x => x.P).ToList();

            return productionPlan;
        }

        /// <summary>
        /// Get the required powerplants for specified payload
        /// </summary>
        /// <param name="payload"></param>
        /// <returns>A list of powerplants that are required for provided payload</returns>
        private List<Powerplant> GetRequiredPowerplantsForPayload(Payload payload)
        {
            // Order all powerplants by PricePerMWh
            var powerplants = payload.Powerplants.OrderBy(x => x.PricePerMWh).ToList();

            // Remaining load for calculating which powerplant is required
            var remainingLoad = payload.Load;

            // List used for return value
            var requiredPowerplants = new List<Powerplant>();

            // Keep looping untill all powerplants have been checked
            while (powerplants.Count != 0)
            {
                var powerplant = powerplants.First();

                if (remainingLoad <= 0)
                {
                    powerplants.Remove(powerplant);
                }
                // If remaining load can NOT be covered by powerplant (Pmin), find the most efficient powerplant that can
                else if (remainingLoad < powerplant.Pmin)
                {
                    var mostEfficientPowerplant = GetMostEfficientPowerplantForRemainingLoad(remainingLoad, powerplants, requiredPowerplants);
                    requiredPowerplants.Add(mostEfficientPowerplant);
                    remainingLoad -= mostEfficientPowerplant.Pmax;
                    powerplants.Remove(mostEfficientPowerplant);
                }
                // If remaining load can NOT be covered by powerplant, because Pmax is too low
                else if (remainingLoad > powerplant.Pmax)
                {
                    requiredPowerplants.Add(powerplant);
                    remainingLoad -= powerplant.Pmax;
                    powerplants.Remove(powerplant);
                }
                // If remaining load is covered by powerplant
                else if (remainingLoad <= powerplant.Pmax)
                {
                    requiredPowerplants.Add(powerplant);
                    remainingLoad -= remainingLoad;
                    powerplants.Remove(powerplant);
                }
            }
            return requiredPowerplants;
        }

        /// <summary>
        /// Add to the provided productionplan the provided powerplants 
        /// </summary>
        /// <param name="productionPlan"></param>
        /// <param name="payload"></param>
        /// <param name="requiredPowerplants"></param>
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

        /// <summary>
        /// Divide the provided load efficiently between the provided required powerplants
        /// </summary>
        /// <param name="productionPlan"></param>
        /// <param name="requiredPowerplants"></param>
        /// <param name="load"></param>
        private void DivideLoadEfficientBetweenRequiredPowerplants(ProductionPlan productionPlan, List<Powerplant> requiredPowerplants, int load)
        {
            var powerplantDeliveries = productionPlan.PowerplantDeliveries.ToList();

            var remainingLoad = load - powerplantDeliveries.Sum(x => x.P);

            // If remaining load is 0 or negative, no spreading is necessary
            if (remainingLoad <= 0) return;

            for (var i = 0; i < requiredPowerplants.Count; i++)
            {
                var powerplant = requiredPowerplants[i];
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

        /// <summary>
        /// Get the most efficient powerplant for the provided remaining load
        /// </summary>
        /// <param name="remainingLoad"></param>
        /// <param name="powerplants"></param>
        /// <param name="usedPowerplants"></param>
        private Powerplant GetMostEfficientPowerplantForRemainingLoad(int remainingLoad, List<Powerplant> powerplants, List<Powerplant> usedPowerplants)
        {
            // For every powerplant we calculate the price it would cost them to cover the remaining load
            // Keeping in mind that if Pmin > remainingLoad, used powerplants can lower their power for cost efficiency
            for (var i = 0; i < powerplants.Count; i++)
            {
                var powerplant = powerplants[i];
                // If Pmin = 0, calculating price for remaining load is easy
                if (powerplant.Pmin == 0)
                {
                    powerplant.PriceRemainingLoad = remainingLoad * powerplant.PricePerMWh;
                }
                // If Pmin > 0 check if used powerplants can lower power and what the cost would be
                else
                {
                    var missingLoad = GetMissingLoad(powerplant, remainingLoad);
                    // If not missing load, price is just remaining load * price per MWh
                    if (missingLoad == 0)
                    {
                        powerplant.PriceRemainingLoad = remainingLoad * powerplant.PricePerMWh;
                    }
                    // If there is missing load, calculate the cost for Pmin and subtract the costs it can use from used powerplants
                    else
                    {
                        powerplant.PriceRemainingLoad = CalculateTotalCostEfficiently(powerplant, usedPowerplants, remainingLoad);
                    }
                }
            }

            return powerplants.OrderBy(x => x.PriceRemainingLoad).First();
        }

        /// <summary>
        /// Calculate the total cost efficiently for the provided powerplant
        /// </summary>
        /// <param name="powerplant"></param>
        /// <param name="usedPowerplants"></param>
        /// <param name="remainingLoad"></param>
        private decimal CalculateTotalCostEfficiently(Powerplant powerplant, List<Powerplant> usedPowerplants, int remainingLoad)
        {
            var pricePmin = powerplant.Pmin * powerplant.PricePerMWh;
            var usedPowerplantsOrderDescByPricePerMWh = usedPowerplants.OrderByDescending(x => x.PricePerMWh).ToList();
            var loadNeededFromStartedPowerplants = powerplant.Pmin - remainingLoad;
            var totalCostForMissingLoad = pricePmin;

            for (var j = 0; j < usedPowerplantsOrderDescByPricePerMWh.Count; j++)
            {
                var startedPowerplant = usedPowerplantsOrderDescByPricePerMWh[j];
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
            return totalCostForMissingLoad;
        }


        /// <summary>
        /// Calculate the missing load for provided powerplant
        /// </summary>
        /// <param name="powerplant"></param>
        /// <param name="remainingLoad"></param>
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

        /// <summary>
        /// Calculate the price per MWh for provided powerplants
        /// </summary>
        /// <param name="payload"></param>
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

        /// <summary>
        /// Calculate the Pmax for the provided winturbines
        /// </summary>
        /// <param name="payload"></param>
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

        /// <summary>
        /// Error handle for the provided payload
        /// </summary>
        /// <param name="payload"></param>
        private void ErrorHandling(Payload payload)
        {
            
            if (payload.Fuels.GasPricePerMWh < 0)
            {
                throw new ArgumentException("Gas price per MWh is negative");
            }
            if (payload.Fuels.KerosinePricePerMWh < 0)
            {
                throw new ArgumentException("Kerosina price per MWh is negative");
            }
            if (payload.Fuels.CO2PerTon < 0)
            {
                throw new ArgumentException("CO2 price per ton is negative");
            }
            if (payload.Fuels.WindPercentage < 0)
            {
                throw new ArgumentException("Wind percentage is negative");
            }
            if (payload.Powerplants.Any(x => x.Pmax == 0))
            {
                var powerplant = payload.Powerplants.Where(x => x.Pmax == 0).First();
                throw new ArgumentException($"Powerplant {powerplant.Name} contains an invalid Pmax");
            }
            if (payload.Powerplants.Any(x => x.Pmax - x.Pmin < 0))
            {
                var powerplant = payload.Powerplants.Where(x => x.Pmax - x.Pmin < 0).First();
                throw new ArgumentException($"Powerplant {powerplant.Name} contains a Pmin higher than Pmax");
            }

            if (payload.Powerplants.Any(x => x.Pmax - x.Pmin < 0))
            {
                var powerplant = payload.Powerplants.Where(x => x.Pmax - x.Pmin < 0).First();
                throw new ArgumentException($"Powerplant {powerplant.Name} contains a Pmin higher than Pmax");
            }
            if (payload.Powerplants.Any(x => x.Efficiency <= 0))
            {
                var powerplant = payload.Powerplants.Where(x => x.Efficiency <= 0).First();
                throw new ArgumentException($"Powerplant {powerplant.Name} contains an invalid efficiency");
            }
            if (payload.Powerplants.Any(x => string.IsNullOrEmpty(x.Name)))
            {
                throw new ArgumentException($"Not all powerplants have a name");
            }
            if (payload.Load <= 0)
            {
                throw new ArgumentException("Load is invalid");
            }
            if (payload.Load > payload.Powerplants.Sum(x => x.Pmax))
            {
                throw new ArgumentException("Load is bigger than the sum powerplants Pmax");
            }
        }
    }
}
