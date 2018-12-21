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
    public class BoardsList
    {
        public List<Board> board { get; set; }
        public string TeamName { get; set; }

    }

    public class Boards
    {
        public string id { get; set; }
        public string name { get; set; }
    }

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

    public class Teams
    {
        public string name { get; set; }
        public string description { get; set; }
    }
    #endregion


    internal class Program
    {
        private static void Main(string[] args)
        {
            string personalAccessToken = "glljbia2pjadsd7mjotsrxs3rxiwcl5t2lzxeixdi7msf5x64ukq";// "xfhozufmatsn6c26tbp6ejhqzvtlwcqqk7gh3l4blqgsttntifna";
            string credentials = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", "", personalAccessToken)));
            string projectName = "Test3";
            string VersionNumber = "?api-version=5.0-preview.1";

            Console.WriteLine("Reading JSON File");
            BoardsList _boardsList = new BoardsList();
            List<Teams> _teams = new List<Teams>();

            using (StreamReader r = new StreamReader("..\\..\\Template\\Boards.json"))
            {
                string json = r.ReadToEnd();
                _boardsList = JsonConvert.DeserializeObject<BoardsList>(json);
            }
            using (StreamReader r = new StreamReader("..\\..\\Template\\Teams.json"))
            {
                string json = r.ReadToEnd();
                _teams = JsonConvert.DeserializeObject<List<Teams>>(json);
            }

            Console.WriteLine("Finished Reading...Provisioning Now");

            //looping throw teams and taking one team at a time
            foreach (var team in _teams)
            {
                string TeamasJSON = JsonConvert.SerializeObject(team);
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("https://dev.azure.com/sachinraj");  //url of your organization

                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

                    var jsonContent = new StringContent(TeamasJSON, Encoding.UTF8, "application/json");
                    var request = client.PostAsync("/_apis/projects/" + projectName + "/teams" + VersionNumber, jsonContent);
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
                    //Here directly we can't update the board columns, since we can't delete existing incoming and outgoing columns.
                    // We can fetch the existing column IDs for the Team Project and Update the JSON with those IDs to respective incoming and outgoing columns
                    //Then we can update the Board

                    //Setting Board Options
                    foreach (Board _board in _boardsList.board)
                    {
                        string boardoptions = JsonConvert.SerializeObject(_board.columns);
                        var boardAsJson = new StringContent(boardoptions, Encoding.UTF8, "application/json");
                        var request1 = client.PutAsync("https://dev.azure.com/devaccounts/" + projectName + "/" + team.name + "/_apis/work/boards/" + _board.name + "/columns" + VersionNumber, boardAsJson);
                        var resp = request1.Result;
                        if (request1.Result.IsSuccessStatusCode)
                        {
                            Console.WriteLine(String.Format("Sucessfully Created Board {0} for team {1}", _board.name, team.name));
                        }
                    }
                }
            }

            Console.WriteLine("Finished Reading... Press a Key to end");
            Console.ReadKey();

        }

    }

}
