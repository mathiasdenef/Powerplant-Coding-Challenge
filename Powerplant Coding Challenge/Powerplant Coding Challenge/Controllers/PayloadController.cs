using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Business.Interfaces;
using Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Powerplant_Coding_Challenge.Hubs;

namespace Powerplant_Coding_Challenge.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class PayloadController : ControllerBase
    {
        private readonly IProductionPlanService _productionPlanService;
        private readonly IHubContext<ProductionPlanHub> _productionPlanHub;

        public PayloadController(IProductionPlanService productionPlanService, IHubContext<ProductionPlanHub> productionPlanHub)
        {
            _productionPlanService = productionPlanService;
            _productionPlanHub = productionPlanHub;
        }

        // POST: api/Payload
        [HttpPost]
        public ProductionPlan GetProductionPlanByPayload([FromBody] Payload payload)
        {
            var productionPlan = _productionPlanService.GetProductionPlanForPayload(payload);
            _productionPlanHub.Clients.All.SendAsync("ReceiveProductionPlan", productionPlan);
            return productionPlan;
        }
    }
}
