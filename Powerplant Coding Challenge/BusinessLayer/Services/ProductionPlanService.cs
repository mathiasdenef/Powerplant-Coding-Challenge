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
        public ProductionPlan GetProductionPlanByPayload(Payload payload)
        {
            // Calculate priceOneMWh (wind = 0)
            CalculatePriceOneMWh(payload);

            CalculateWindPmax(payload);

            // Order by priceOneMWh
            var orderedPowerplants = payload.Powerplants.OrderBy(x => x.PriceOneMWh).ToList();

            // Create ProductionPlanObject
            var productionPlan = new ProductionPlan
            {
                PowerplantNameAndPowers = new Collection<PowerplantNameAndPower>()
            };

            // Fill ProductionPlanObject based on pMin and pMax
            // If load between pMin and pMax => add to list with p = load
            // If load bigger than pMax and next load can be efficiently covered by next powerplant => add to list with p = pMax
            // If load bigger than pMax and next load is not efficiently covered by next powerplant => calculate efficiently spreading
            // If load smaller than pMin => add to list with p = pMin
            var load = payload.Load;

            for (int i = 0; i < orderedPowerplants.Count; i++)
            {
                var powerplant = orderedPowerplants[i];

                // If load is 0, add to list with p = 0
                if (load <= 0)
                {
                    AddToProductionPlan(productionPlan, powerplant.Name, 0);
                }
                // If between pMin and pMax, add to list with p = load
                else if (load >= powerplant.Pmin && load <= powerplant.Pmax)
                {
                    AddToProductionPlan(productionPlan, powerplant.Name, load);
                    load -= load;
                }
                // If bigger than pMax
                else if (load > powerplant.Pmax)
                {
                    // If powerplant is not last in list
                    if (i != orderedPowerplants.Count)
                    {
                        var nextPowerplant = orderedPowerplants[i + 1];
                        // If next load is covered => add to list with p = pMax
                        if ((load - powerplant.Pmax) >= nextPowerplant.Pmin)
                        {
                            AddToProductionPlan(productionPlan, powerplant.Name, powerplant.Pmax);
                            load -= powerplant.Pmax;
                        }
                        // If next load is not covered => check what most efficient spread is
                        else
                        {
                            var nextMostEfficientPowerplant = FindNextMostEfficientPowerplantForLoad(orderedPowerplants.Skip(i).ToList(), load);

                            // If there is no more efficient powerplant => add to list with p = pMax and next one will be p = pMin
                            if (nextMostEfficientPowerplant == null)
                            {
                                AddToProductionPlan(productionPlan, powerplant.Name, powerplant.Pmax);
                                load -= powerplant.Pmax;
                            }
                            else
                            {
                                //AddToProductionPlan(productionPlan, nextMostEfficientPowerplant.Name, nextMostEfficientPowerplant.Pmax);
                                //load -= powerplant.Pmax;
                            }
                        }
                    }
                    // If this is the last powerplant, add to list with p = pMax and the remaining load can't be covered
                    else
                    {
                        AddToProductionPlan(productionPlan, powerplant.Name, load);
                        load -= powerplant.Pmax;
                    }
                    //// If next load is covered => add to list with p = pMax
                    //if (nextPowerplant != null && (load - powerplant.Pmax) >= nextPowerplant.Pmin)
                    //{
                    //    AddToProductionPlan(productionPlan, powerplant.Name, powerplant.Pmax);
                    //    notUsedPowerplants.RemoveAll(x => x.Name == powerplant.Name);
                    //    load -= powerplant.Pmax;
                    //}
                    //// If next load is not covered => find the most efficient spread possible
                    //else
                    //{
                    //    var nextMostEfficientPowerplant = FindNextMostEfficientPowerSpread(notUsedPowerplants, load);

                    //    var missingLoad = nextPowerplant.Pmin - (load - powerplant.Pmax);


                    //    // Add load with p = pMax - missingLoad
                    //    productionPlan.PowerplantNameAndPowers.Add(new PowerplantNameAndPower()
                    //    {
                    //        Name = powerplant.Name,
                    //        P = (powerplant.Pmax - missingLoad)
                    //    });
                    //    load -= (powerplant.Pmax - missingLoad);




                    //    // try to spread efficiently

                    //    // Calculate the cost that will be added too much in next powerplant
                    //    // Lower the current powerplant and add it to next one


                    //}

                }
                // If smaller than pMin, add to list with p = pMax
                else if (load < powerplant.Pmin)
                {
                    AddToProductionPlan(productionPlan, powerplant.Name, powerplant.Pmin);
                    load -= powerplant.Pmin;
                }

            }

            // If adding load is bigger than pMax, check if next one is efficient



            // 2) Order the powerplants by efficiency

            // 3) Use the most efficient powerplants first

            // 4) Before using the powerplants MWh, check if the rest can be covered efficiently otherwise use another powerplant





            return productionPlan;
        }

        private Powerplant FindNextMostEfficientPowerplantForLoad(List<Powerplant> powerplants, int load)
        {
            // First in list is what i'm currently looping
            var currentPowerplant = powerplants[0];
            var remainingLoad = load - currentPowerplant.Pmax;
            Powerplant nextMostEfficientPowerplant = null;

            for (var i = 1; i < powerplants.Count; i++)
            {
                //var powerplant = powerplants[i];
                //if ((powerplant.Pmin - remainingLoad) > 0)
                //var missingLoadToPmin = powerplant.Pmin - remainingLoad;
                //var costPowerplantMissingLoadToPmin = missingLoadToPmin * powerplant.PriceOneMWh;
                //var costCurrentPowerplantMissingLoadToPmin = missingLoadToPmin * currentPowerplant.PriceOneMWh;

                //// If cost of powerplant is lower than currentPowerplant => powerplant is nextMostEfficientPowerplant
                //if (costPowerplantMissingLoadToPmin < costCurrentPowerplantMissingLoadToPmin)
                //{
                //    nextMostEfficientPowerplant = powerplant;
                //}
            }

            return nextMostEfficientPowerplant;
        }

        private void CalculatePriceOneMWh(Payload payload)
        {
            foreach (var powerplant in payload.Powerplants)
            {
                var fuelPrice = GetFuelByPowerplantType(payload.Fuels, powerplant.Type);

                if (powerplant.Type == PowerplantType.Gasfired || powerplant.Type == PowerplantType.Turbojet)
                {
                    powerplant.PriceOneMWh = fuelPrice / powerplant.Efficiency;
                }
                else
                {
                    // Wind has no cost
                    powerplant.PriceOneMWh = 0;
                }
            }
        }

        private void CalculateWindPmax(Payload payload)
        {
            var windturbinePowerplants = payload.Powerplants.Where(x => x.Type == PowerplantType.Windturbine);
            foreach (var powerplant in windturbinePowerplants)
            {
                var windPercentage = GetFuelByPowerplantType(payload.Fuels, PowerplantType.Windturbine);

                // Using int so it's rounded automatically
                powerplant.Pmax = decimal.ToInt32((powerplant.Pmax * windPercentage) / 100);
            }
        }

        private void AddToProductionPlan(ProductionPlan productionPlan, string name, int p)
        {
            productionPlan.PowerplantNameAndPowers.Add(new PowerplantNameAndPower()
            {
                Name = name,
                P = p
            });
        }

        private decimal GetFuelByPowerplantType(Fuels fuels, PowerplantType type)
        {
            switch (type)
            {
                case PowerplantType.Gasfired:
                    return fuels.Gas;
                case PowerplantType.Turbojet:
                    return fuels.Kerosine;
                case PowerplantType.Windturbine:
                    return fuels.Wind;
                default:
                    throw new Exception("No correct PowerplantType used.");
            }
        }
    }
}
