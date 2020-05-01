using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Powerplant_Coding_Challenge.Hubs
{
    public class ProductionPlanHub : Hub
    {

        public async Task NewMessage(Message msg)
        {
            await Clients.All.SendAsync("MessageReceived", msg);
        }

        public struct WebSocketActions
        {
            public static readonly string MESSAGE_RECEIVED = "messageReceived";
            public static readonly string USER_LEFT = "userLeft";
            public static readonly string USER_JOINED = "userJoined";
        }


        public class Message
        {
            public string clientuniqueid { get; set; }
            public string type { get; set; }
            public string message { get; set; }
            public DateTime date { get; set; }
        }
    }
}
