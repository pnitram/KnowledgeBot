using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using VismaBot.Dialogs;
using VismaBot.Models;

namespace VismaBot.Controllers
{
    public class HangoutController : ApiController
    {
        private string apikey;

        public HangoutController()
        {
            apikey = Environment.GetEnvironmentVariable("apikey");
        }

        [HttpGet]
        [Route("api/DDSSAwdssssa")]
        public string DDSSAwdssssa()
        {
            return "Connected";
        }

        [HttpPost]
        [Route("api/DDSSAwdssssa")]
        public ResponseMessage DDSSAwdssssa([FromBody]ChatRequest data)
        {
            if (data.token != apikey)
            {
                return new ResponseMessage()
                {
                    text = "Invalid API key",
                    thread = data.message.thread
                };
            }

            var response = new HandleMessage();
            return new ResponseMessage()
            {
                text = response.CreateResponseMessage(data),
                thread = data.message.thread
            };
        }

    }
}
