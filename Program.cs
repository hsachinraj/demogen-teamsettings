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
        public bool defaultTeam {get; set;}
    }

    public class TeamSettings
    {
        public string bugsBehavior { get; set; }
        public BacklogVisibilities backlogVisibilities { get; set; }
    }

    public class BacklogVisibilities
    {
        public bool EpicCategory { get; set; }
}
    #endregion


    internal class Generator
    {
        

        public static void Main(string[] args)
        {
            string personalAccessToken = "xfhozufmatsn6c26tbp6ejhqzvtlwcqqk7gh3l4blqgsttntifna";
            string credentials = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", "", personalAccessToken)));
            string projectName = "Test3";
            string acctName = "sachinraj";
            string VersionNumber = "?api-version=5.0-preview.1";
            string selectedTemplate = "Demo Template";
            string defaultTeam = projectName+" Team";
            string templatePath = "..\\..\\Template\\" + selectedTemplate + "\\";
            var client = new HttpClient();

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

            //looping throw teams and taking one team at a time
            foreach (var team in _teams)
            {
               
                string TeamasJSON = JsonConvert.SerializeObject(team);
                string teamFolder = Path.GetFullPath(templatePath + "Teams\\"+team.folder); ;
                Console.WriteLine("---------------------------");
                
                //create the team if it is not default; for default teams, we will just retain the settings
                if (team.defaultTeam ==false)
                {
                    Console.WriteLine(String.Format("Creating Team {0}", team.name);

                   client.BaseAddress = new Uri("https://dev.azure.com/");  //url of your organization

                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
                    CreateTeam(client, team.name);
                    if (Directory.Exists(teamFolder))
                    {
                        if (File.Exists(teamFolder+"//TeamSettings.json"))
                        {
                            CreateTeamSetting(client,)
                        }
                    }
                }
                

                    if (Directory.Exists(teamFolder))
                    {
                        Console.WriteLine(String.Format("Team {0} has settings in folder {1}", team.name, teamFolder));
                    }
                    else
                    {
                        Console.WriteLine(String.Format("Team {0} does not have settings ", team.name));
                    }

                }
            }

        private static void CreateTeam(HttpClient client, string name)
        {
            using (client)
            {


                var jsonContent = new StringContent(TeamasJSON, Encoding.UTF8, "application/json");
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

        Console.WriteLine("Finished Reading... Press a Key to end");
            Console.ReadKey();

        }

    }

}
