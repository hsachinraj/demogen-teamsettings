using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace ConsoleApp2
{
    #region defineBoards
    //public class BoardsList
    //{
    //    public List<Board> board { get; set; }
    //    public string TeamName { get; set; }

    //}

    //public class boards
    //{
    //    public string id { get; set; }
    //    public string name { get; set; }
    //    public list<board> board { get; set; }
    //}

    public class Board
    {
        public bool canEdit { get; set; }
        public string id { get; set; }

        public bool isValid { get; set; }
        public string name { get; set; }
        public int revision { get; set; }

        public List<BoardColumn> columns { get; set; }

        public List<BoardFields> fields { get; set; }

        public List<BoardRow> rows { get; set; }
    }

    public class BoardColumn
    {
        public string name { get; set; }
        public int itemLimit { get; set; }
        public string id { get; set; }
        public bool isSplit { get; set; }
        public string description { get; set; }
        public string columnType { get; set; }

    }
    public class BoardFields
    {
        public FieldReference columnField { get; set; }
        public FieldReference doneField { get; set; }
        public FieldReference rowField { get; set; }

    }
    public class BoardRow
    {
        public string id { get; set; }
        public string name { get; set; }
    }


    public class FieldReference
    {
        public string referenceName { get; set; }
    }

    public class Team
    {
        public string name { get; set; }
        public string description { get; set; }
        public string folder { get; set; }
        public bool defaultTeam { get; set; }
    }

    public class TeamSettings
    {
        public string BoardType { get; set; }
        public string BugsBehavior { get; set; }
        public BacklogVisibilities BacklogVisibilities { get; set; }
    }

    public class BacklogVisibilities
    {

        [JsonProperty(PropertyName = "Microsoft.EpicCategory")]
        public bool EpicCategory { get; set; }

        [JsonProperty(PropertyName = "Microsoft.FeatureCategory")]
        public bool FeatureCategory { get; set; }

        [JsonProperty(PropertyName = "Microsoft.RequirementCategory")]
        public bool RequirementCategory { get; set; }
    }

   /* public class Iterations
    {
        [JsonProperty(PropertyName = "IterationsList")]
        public Iteration[] IterationsList { get; set; }


    }*/

    public class Iterations
    {
        public string Team { get; set; }
        [JsonProperty(PropertyName = "value")]
        public List<IterationValue> Value { get; set; }

    }
    public class IterationValue{
        public string Name { get; set; }
        public string Path { get; set; }

        public TeamIterationAttributes attributes { get; set; }
    }
    public class TeamIterationAttributes
    {
        public string startDate { get; set; }
        public string finishDate { get; set; }
        public string timeFrame { get; set; }
    }
    #endregion


    internal class Generator
    {
        private static string acctName = "sachinraj";
        private static string personalAccessToken = "xfhozufmatsn6c26tbp6ejhqzvtlwcqqk7gh3l4blqgsttntifna";
        private static string credentials = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", "", personalAccessToken)));
        private static string projectName = "Test3";
        private static string VersionNumber = "?api-version=5.0-preview.1";
        private static string selectedTemplate = "SmartHotel360";
        private static string defaultTeam = projectName + " Team";
        private static string templatePath = "..\\..\\Template\\" + selectedTemplate + "\\";

        public static void Main(string[] args)
        {



            Console.WriteLine("Reading JSON File");
            List<Board> _boardsList = new List<Board>();
            List<Team> _teams = new List<Team>();

            /* using (StreamReader r = new StreamReader("..\\..\\Template\\Boards.json"))
             {
                 string json = r.ReadToEnd();
                 _teams = JsonConvert.DeserializeObject<List<Team>>(json);
             }
             */

            using (StreamReader r = new StreamReader(templatePath + "Teams\\Teams.json"))
            {
                string json = r.ReadToEnd();
                _teams = JsonConvert.DeserializeObject<List<Team>>(json);
            }


            Console.WriteLine("Finished Reading...Provisioning Now");
            string teamNameToProvision = "";
            //looping throw teams and taking one team at a time
            foreach (var team in _teams)
            {

                string teamAsJson = JsonConvert.SerializeObject(team);
                string teamFolder = Path.GetFullPath(templatePath + "Teams\\" + team.name); ;
                Console.WriteLine("---------------------------");

                //create the team if it is not default; for default teams, we will just update the settings
                if (team.defaultTeam)
                {
                    teamNameToProvision = defaultTeam;
                    Console.WriteLine(String.Format("Team {0} is the default...skipping creation. This will be provisoned as {1}", team.name, teamNameToProvision));

                }
                else
                {
                    teamNameToProvision = team.name;
                    Console.WriteLine(String.Format("Creating Team {0}", teamNameToProvision));
                    CreateTeam(teamAsJson);
                   CreateArea(teamNameToProvision);
                }

                // Team Settings not working
                CreateIterations(Path.GetFullPath(templatePath + "Teams\\Iterations1.json"), team.name,teamNameToProvision);

                string teamSettingsFile = teamFolder + "\\TeamSetting.json";

                if (Directory.Exists(teamFolder))
                {
                    Console.WriteLine("Folder for the team found...Updating Team settings");
                    if (File.Exists(teamSettingsFile))
                    {
                        Console.WriteLine("Updating Team Settings");
                       CreateTeamSetting(teamSettingsFile, teamNameToProvision);
                    }
                }
                else
                {
                    Console.WriteLine(String.Format("Team {0} does not have settings ", team.name));
                }

            }



            Console.WriteLine("Finished Reading... Press a Key to end");
            Console.ReadKey();
        }
    

        private static bool CreateTeamSetting( string teamSettingsFile, string teamName)
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri("https://dev.azure.com/");  //url of your organization
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
           List<TeamSettings> teamSettings = new List<TeamSettings>();
            string teamSettingsJson = "";

            using (StreamReader r = new StreamReader(teamSettingsFile))
            {
                teamSettingsJson = r.ReadToEnd();
                teamSettings = JsonConvert.DeserializeObject<List<TeamSettings>>(teamSettingsJson);
            }

            using (client)
            {
                var jsonContent = new StringContent(teamSettingsJson, Encoding.UTF8, "application/json");
                var method = new HttpMethod("PATCH");
                var request = new HttpRequestMessage(method, acctName + "//" + projectName + "//" + teamName + "/_apis/work/teamsettings" + VersionNumber) { Content = jsonContent };
                HttpResponseMessage response = new HttpResponseMessage();
                try
                {
                    response = client.SendAsync(request).Result;
                    Console.WriteLine(String.Format("Successfully updated settings for {0} Team ",teamName ));
                }
                catch (Exception e)
                {
                    Console.WriteLine(String.Format("Error creating team due to {0} with message {1}", response.StatusCode, e.ToString()));
                    return false;
                }
                return true;

            }
        }

        private static void CreateTeam(string teamAsJson)
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri("https://dev.azure.com/");  //url of your organization
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

            using (client)
            {
                var jsonContent = new StringContent(teamAsJson, Encoding.UTF8, "application/json");
                var request = client.PostAsync(acctName + "/_apis/projects/" + projectName + "/teams" + VersionNumber, jsonContent);
                var response = request.Result;

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Successfully Created Team ");
                }
                else
                {
                    Console.WriteLine(String.Format("Error creating team due to {0}", response.StatusCode));
                    return;
                }
            }



        }


        private static bool CreateIterations(string IterationsFile, string teamNameToSearch, string teamNameToProvision)
        {
            var iterationsJson="";
            List<Iterations> _iterations = new List<Iterations>();

            var client = new HttpClient();
            client.BaseAddress = new Uri("https://dev.azure.com/");  //url of your organization
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
            HttpResponseMessage response = new HttpResponseMessage();
            

            using (StreamReader r = new StreamReader(IterationsFile))
            {
                iterationsJson = r.ReadToEnd();
                _iterations = JsonConvert.DeserializeObject<List<Iterations>>(iterationsJson);
            }



            foreach (Iterations _iteration in _iterations)
            {
                if (_iteration.Team == teamNameToSearch)
                {
                    using (client)
                    {
                        string iterationAsJson = JsonConvert.SerializeObject(_iteration.Value);
                        var jsonContent = new StringContent(iterationAsJson, Encoding.UTF8, "application/json");
                        var request = client.PostAsync(acctName + "/" + projectName + "/_apis/work/teamsettings/iterations" + VersionNumber, jsonContent);
                        try
                        {
                           
                            response = request.Result;
                            if (response.IsSuccessStatusCode)
                            {
                                Console.WriteLine(String.Format("Successfully updated Iterations for {0} Team ", teamNameToProvision));
                            }
                            
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(String.Format("Error creating Iterations due to {0} with message {1}", response.StatusCode, e.ToString()));
                            return false;
                        }
                    }
                }
            }
                return true;


        }

        public static bool CreateArea(string areaName)

        {
            string createdAreaName = string.Empty;
            object node = new { name = areaName };

            var client = new HttpClient();
            client.BaseAddress = new Uri("https://dev.azure.com/");  //url of your organization
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
            HttpResponseMessage response = new HttpResponseMessage();

            using (client)
            {

                var jsonContent = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(node), Encoding.UTF8, "application/json");
                var method = new HttpMethod("PATCH");
                var request = new HttpRequestMessage(method, acctName + "/" + projectName + "/_apis/wit/classificationNodes/areas" + "?api - version = 5.1 - preview.2") { Content = jsonContent } ;
                response = client.SendAsync(request).Result;
                if (response.IsSuccessStatusCode)
                {
                    string result = response.Content.ReadAsStringAsync().Result;
                    return true;
                }
            }
            return false;
        }
    }

}
