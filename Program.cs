using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
    }
    #endregion


    class Program
    {

        static void Main(string[] args)
        {
            string personalAccessToken = "xfhozufmatsn6c26tbp6ejhqzvtlwcqqk7gh3l4blqgsttntifna";
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

            string TeamasJSON = JsonConvert.SerializeObject(_teams);

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

                //Setting Board Options
                foreach (Board _board in _boardsList.board)
                {
                    string boardoptions = JsonConvert.SerializeObject(_board);
                    var boardAsJson = new StringContent(boardoptions, Encoding.UTF8, "application/json");
                    var request1 = client.PutAsync(projectName + "/" + teamName + "/_apis/work/boards/" + _board.name + VersionNumber, boardAsJson);
                    if (request1.Result.IsSuccessStatusCode)
                    {
                        Console.WriteLine(String.Format("Sucessfully Created Board {0} for team {1}", _board.name, teamName));
                    }

                }



            }


            Console.WriteLine("Finished Reading... Press a Key to end");
            Console.ReadKey();

        }

    }

}
