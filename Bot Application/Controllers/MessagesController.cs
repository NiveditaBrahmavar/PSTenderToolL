using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
//using Microsoft.Bot.Connector.Utilities;
using Newtonsoft.Json;
using Bot_Application.Model;
using Bot_Application.Util;
using System.Configuration;

namespace Bot_Application
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        private readonly ITenderBot tenderBot;

        public MessagesController()
        {
            tenderBot = new TenderBot();
        }       
     
        public async Task<Message> Post([FromBody]Message message)
        {    
           
            if (message.Type == "Message")
            {                
                string result= string.Empty;      

                TenderLUIS tenderLUIS = await GetEntityFromLUIS(message.Text);
                if (tenderLUIS.intents.Count() > 0)
                {
                    switch (tenderLUIS.intents[0].intent)
                    {                        
                        case "TenderStatus":
                            if (tenderLUIS.entities.Count() > 0)
                            {
                                if (CheckEntity(tenderLUIS.entities[0].entity))
                                {
                                    result = tenderBot.FetchTenderStatus(Convert.ToInt32(tenderLUIS.entities[0].entity));
                                }
                            }
                            else
                                result = Constants.INCORRECT_MSG;
                            break;
                        case "CurrentOwner":
                            if (tenderLUIS.entities.Count() > 0)
                            {
                                if (CheckEntity(tenderLUIS.entities[0].entity))
                                {
                                    result = tenderBot.FetchCurrentOwner(Convert.ToInt32(tenderLUIS.entities[0].entity));
                                }
                            }
                            else
                                result = Constants.INCORRECT_MSG;
                            break;
                        case "TenderAnnouncementDate":
                            if (tenderLUIS.entities.Count() > 0)
                            {
                                if (CheckEntity(tenderLUIS.entities[0].entity))
                                {
                                    result = tenderBot.FetchAnnouncementDate(Convert.ToInt32(tenderLUIS.entities[0].entity));
                                }
                            }
                            else
                                result = Constants.INCORRECT_MSG;
                            break;
                        case "TenderDetails":
                            if (tenderLUIS.entities.Count() > 0)
                            {
                                if (CheckEntity(tenderLUIS.entities[0].entity))
                                {
                                    result = tenderBot.FetchTenderDetails(Convert.ToInt32(tenderLUIS.entities[0].entity));
                                }
                            }
                            else
                                result = Constants.INCORRECT_MSG;
                            break;
                        case "TenderListPrevWeek":
                            result = tenderBot.FetchPreviousTenders();
                            if (string.IsNullOrEmpty(result))
                                result = "No tenders announced last week.";
                            break;
                        default:
                            result = Constants.INVALID_MSG;
                            break;
                    }
                }
                else
                {
                    result = Constants.INVALID_MSG;
                }
                
                // return our reply to the user  
                return message.CreateReplyMessage(result);
            }
            else
            {
                return HandleSystemMessage(message);
            }
        }

        private Message HandleSystemMessage(Message message)
        {
            if (message.Type == "Ping")
            {
                Message reply = message.CreateReplyMessage();
                reply.Type = "Ping";
                return reply;
            }
            else if (message.Type == "BotAddedToConversation")
            {
                return message.CreateReplyMessage("Welcome to PS Tender Tool HelpDesk." + Environment.NewLine + "We can help you with the following details" + Environment.NewLine + "1. Get Stage of Tender Id" + Environment.NewLine + "2. Get Current owner of the Tender Id" + Environment.NewLine + "3. Get Current owner of the Tender Id" + Environment.NewLine + "4. Get announcement Date for Tender Id" + Environment.NewLine + "5. Get Details of Tender Id" + Environment.NewLine + "6. Get List of Tenders announced Last week");                
            }           
            else if (message.Type == "BotRemovedFromConversation")
            {
            }
            else if (message.Type == "UserAddedToConversation")
            {
            }
            else if (message.Type == "UserRemovedFromConversation")
            {
            }
            else if (message.Type == "EndOfConversation")
            {
            }           

            return null;
        }

        private static async Task<TenderLUIS> GetEntityFromLUIS(string Query)
        {
            Query = Uri.EscapeDataString(Query);            
            TenderLUIS Data = new TenderLUIS();
            using (HttpClient client = new HttpClient())
            {
                string RequestURI = ConfigurationManager.AppSettings["ApiURl"] + Query + "&verbose=true";
                HttpResponseMessage msg = await client.GetAsync(RequestURI);

                if (msg.IsSuccessStatusCode)
                {
                    var JsonDataResponse = await msg.Content.ReadAsStringAsync();
                    Data = JsonConvert.DeserializeObject<TenderLUIS>(JsonDataResponse);
                }
            }
            return Data;
        }
        
        private bool CheckEntity(string entity)
        {
            bool isNumeric;
            int tenderId;

            isNumeric = int.TryParse(entity, out tenderId);

            return isNumeric;
        }       

    }
}