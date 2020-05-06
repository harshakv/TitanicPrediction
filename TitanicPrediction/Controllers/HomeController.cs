using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using TitanicPrediction.Models;

namespace TitanicPrediction.Controllers
{
    public class HomeController : Controller
    {
        private static List<Titanic> titanicList;
        public HomeController()
        {
            DataAccess da = new DataAccess();
            titanicList = da.GetData();
        }
        public ActionResult Index()
        {
            // InvokeRequestResponseService().Wait();
            List<SelectListItem> passengerIdList = new List<SelectListItem>();
            Titanic titanic = new Titanic();

            titanicList.ForEach(x =>
            {
                passengerIdList.Add(new SelectListItem { Text = x.PassengerId, Value = x.PassengerId.ToString() });
            });
            titanic.PassengerIdList = passengerIdList;
            return View(titanic);
        }

        [HttpGet]
        public ActionResult GetPassengerDetails(string passengerId)
        {
            Titanic titanic = (Titanic)titanicList.Where(x => x.PassengerId == passengerId).FirstOrDefault();
            return Json(titanic, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetSurvivalInfo(string passengerId, string pclass, string sex, string age, string sibsp, string parch, string fare, string embarked)
        {
            InvokeRequestResponseService(passengerId, pclass, sex, age, sibsp, parch, fare, embarked).Wait();
            Titanic titanic = (Titanic)ViewData["Titanic"];
            return Json(titanic, JsonRequestBehavior.AllowGet); 
        }
        private async Task InvokeRequestResponseService(string passengerId, string pclass, string sex, string age, string sibsp, string parch, string fare, string embarked)
        {
            using (var client = new HttpClient())
            {
                var scoreRequest = new
                {
                    Inputs = new Dictionary<string, List<Dictionary<string, string>>>() {
                        {
                            "input1",
                            new List<Dictionary<string, string>>(){new Dictionary<string, string>(){
                                            {
                                                "PassengerId", passengerId
                                            },
                                            {
                                                "Pclass", pclass
                                            },
                                            {
                                                "Sex", sex
                                            },
                                            {
                                                "Age", age
                                            },
                                            {
                                                "SibSp", sibsp
                                            },
                                            {
                                                "Parch", parch
                                            },
                                            {
                                                "Fare", fare
                                            },
                                            {
                                                "Embarked", embarked
                                            },
                                }
                            }
                        },
                    },
                    GlobalParameters = new Dictionary<string, string>()
                    {
                    }
                };

                const string apiKey = "/agNeSkvOO8JZVGh4ecGr8Q+A6aq+OpbhhSs9B8E9RDOd8mERiFy/1011ovz/pbx5Z4HJf5C1VEoKbUjW+IAZQ=="; // Replace this with the API key for the web service
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                client.BaseAddress = new Uri("https://ussouthcentral.services.azureml.net/workspaces/96c46a03abf0427bbb1b1f5fac14ed45/services/ccfbcea874ae45a48eaed790010d7f03/execute?api-version=2.0&format=swagger");

                // WARNING: The 'await' statement below can result in a deadlock
                // if you are calling this code from the UI thread of an ASP.Net application.
                // One way to address this would be to call ConfigureAwait(false)
                // so that the execution does not attempt to resume on the original context.
                // For instance, replace code such as:
                //      result = await DoSomeTask()
                // with the following:
                //      result = await DoSomeTask().ConfigureAwait(false)

                HttpResponseMessage response = await client.PostAsJsonAsync("", scoreRequest).ConfigureAwait(false);

                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();

                    //JavaScriptSerializer serializer = new JavaScriptSerializer();
                    //var jsonObject = serializer.Deserialize<dynamic>(result);
                    Titanic titanic = new Titanic();

                    JObject o = JObject.Parse(result);

                    titanic.PassengerId = ((JValue)o.SelectToken("$.Results.output1[0]")["Passenger Id"]).Value.ToString();
                    titanic.Name = ((JValue)o.SelectToken("$.Results.output1[0]")["Name"]).Value.ToString();
                    titanic.Survived = ((JValue)o.SelectToken("$.Results.output1[0]")["Survived"]).Value.ToString();
                    titanic.ScoredProbability = ((JValue)o.SelectToken("$.Results.output1[0]")["Scored Probabilities"]).Value.ToString();
                    ViewData["Titanic"] = titanic;
                }
                else
                {
                    Console.WriteLine(string.Format("The request failed with status code: {0}", response.StatusCode));

                    // Print the headers - they include the requert ID and the timestamp,
                    // which are useful for debugging the failure
                    Console.WriteLine(response.Headers.ToString());

                    string responseContent = await response.Content.ReadAsStringAsync();
                    //Console.WriteLine(responseContent);
                }
            }
        }
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }

}