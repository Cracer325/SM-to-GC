using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Newtonsoft.Json;


namespace SMtoGC
{

    internal class Program
    {




        public class Schedule
        {
            [JsonProperty("title")]
            public string Title { get; set; }

            [JsonProperty("events")]
            public List<JEvent> Events { get; set; }

            [JsonProperty("settings")]
            public Settings Settings { get; set; }
        }

        public class JEvent
        {
            [JsonProperty("title")]
            public string Title { get; set; }

            [JsonProperty("description")]
            public string Description { get; set; }

            [JsonProperty("day")]
            public int Day { get; set; }

            [JsonProperty("start")]
            public string Start { get; set; }

            [JsonProperty("end")]
            public string End { get; set; }

            [JsonProperty("color")]
            public string Color { get; set; }

            [JsonProperty("icon")]
            public object Icon { get; set; }
        }



        public static string RoundToNearestColorID(string color)
        {
            int[] colorIndexes = { 0x7986CB, 0x33B679, 0x8E24AA, 0xE67C73, 0xF6BF26, 0xF4511E, 0x039BE5, 0x616161, 0x3F51B5, 0x0B8043, 0xD50000};
            int input = Convert.ToInt32(color, 16);

            int minDiff = int.MaxValue;
            int minIndex = 0;
            for(int i = 0; i<colorIndexes.Length; i++)
            {
                if (minDiff > Math.Abs(colorIndexes[i] - input))
                {
                    minDiff = Math.Abs(colorIndexes[i] - input);
                    minIndex = i;
                }
            }
            return (minIndex + 1).ToString();
        }


        public static Event CreateNewEventForGC(int dd, string title, int start_hour, int start_minute, int end_hour, int end_minute, string color, int offset)
        {
            //MAKE SURE TO CHANGE TO YOUR OWN TIME ZONE
            DateTime sunday = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
            string TIMEZ = "GMT";
            Event googleCalendarEvent = new Event();
            int[] days = { int.Parse(sunday.ToString("dd"))+1, int.Parse(sunday.ToString("dd"))+2, int.Parse(sunday.ToString("dd"))+3, int.Parse(sunday.ToString("dd"))+4, int.Parse(sunday.ToString("dd"))+5, int.Parse(sunday.ToString("dd"))+6, int.Parse(sunday.ToString("dd")) };
            //Start date
            googleCalendarEvent.Start = new EventDateTime()
            { DateTimeDateTimeOffset = new DateTimeOffset(2024, 10, days[dd], start_hour, start_minute, 0, TimeSpan.FromHours(offset)),
            TimeZone=TIMEZ};

            //End date
            googleCalendarEvent.End = new EventDateTime()
            { DateTimeDateTimeOffset = new DateTimeOffset(2024, 10, days[dd], end_hour, end_minute, 0, TimeSpan.FromHours(offset)),
            TimeZone = TIMEZ
            };

            
            googleCalendarEvent.Summary = title;
            googleCalendarEvent.Recurrence = new String[] {
          "RRULE:FREQ=WEEKLY"
            };
            googleCalendarEvent.ColorId = RoundToNearestColorID(color.Replace("#", ""));
            return googleCalendarEvent;
        }

        static async Task Main(string[] args)
        {
            //REMEMBER TO CHANGE THE TIMEZONE VARIABLE INSIDE THE CreateNewEventForGC FUNCTION!!!

            //IDS:
            const string calendarId = "primary"; //primary means writing in the main calendar, replace with a calendar ID if you want a different one
            string clientId = "...";
            string clientSecret = "...";
            //Copy paste the contents of the JSON file
            string json = @"";
            Console.WriteLine("Enter the time offset in hours from GMT+0: ");
            int timeZone = int.Parse(Console.ReadLine());
            string[] scopes = { "https://www.googleapis.com/auth/calendar" };

            var credentials = GoogleWebAuthorizationBroker.AuthorizeAsync(
                new ClientSecrets { ClientId = clientId, ClientSecret = clientSecret },
                scopes, "user", CancellationToken.None).Result;

            var service = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credentials
            }
            );

            var schedule = JsonConvert.DeserializeObject<Schedule>(json);

            // Serialize each event inside the events list
            foreach (var eventItem in schedule.Events)
            {
                string thisjson = JsonConvert.SerializeObject(eventItem);
                var currentEvent = JsonConvert.DeserializeObject<JEvent>(thisjson);
                try
                {
                    Event result = await service.Events.Insert(CreateNewEventForGC(currentEvent.Day, currentEvent.Title, int.Parse(currentEvent.Start.Split(':')[0]), int.Parse(currentEvent.Start.Split(':')[1]), int.Parse(currentEvent.End.Split(':')[0]), int.Parse(currentEvent.End.Split(':')[1]), currentEvent.Color, timeZone), calendarId).ExecuteAsync();

                    Console.WriteLine($"Event created: {result.HtmlLink}\n");
                }
                catch (GoogleApiException ex)
                {
                    Console.WriteLine(ex.ToString());
                    Console.WriteLine("Possible reasons for an error:\n- calander ID isnt correct. \n- any of the IDS/Keys arent correct \n- Didn't authorize");
                    break;
                }
            }

            Console.WriteLine("Finished.");
            Console.ReadKey();
        }


    }
}
