using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace EasyProgramAccess
{
    class FirebaseRest
    {
        private string _baseUrl;
        private string _authToken;
        private List<string> _children = new List<string>();
        private List<string> _queries = new List<string>();
        private string _orderBy = "";

        // Expects the url without the .json at the end
        public FirebaseRest(string url, string authenticationToken)
        {
            _baseUrl = url;
            _authToken = authenticationToken;

        }

        // This is used to go lower down in the nesting of the database
        public FirebaseRest Child(string child)
        {
            _children.Add(child);
            return this;
        }

        // This is used to set the orderBy attribute
        public FirebaseRest OrderBy(string ordr)
        {
            _orderBy = "orderBy=\"" + ordr + "\"";
            return this;
        }

        public FirebaseRest EqualTo(string eq)
        {
            _queries.Add("equalTo=\"" + eq + "\"");
            return this;
        }

        public FirebaseRest EqualTo(int eq)
        {
            _queries.Add("equalTo=" + eq);
            return this;
        }



        public FirebaseRest Print(string prnt)
        {
            _queries.Add("print=" + prnt);
            return this;
        }

        public FirebaseRest Pretty()
        {
            return Print("pretty");

        }

        public FirebaseRest Shallow()
        {
            _queries.Add("shallow=true");
            return this;
        }


        public string Build()
        {
            string url = _baseUrl;
            foreach (string child in _children)
            {
                url += "/" + child;
            }
            url += ".json?auth=" + _authToken;
            url += "".Equals(_orderBy) ? "" : "&" + _orderBy;
            foreach (string query in _queries)
            {
                url += "&" + query;
            }

            CleanLists();

            return url;
        }


        // Clears the query and children arrays
        public void CleanLists()
        {
            _children.Clear();
            _queries.Clear();
            _orderBy = "";
        }


        // HTTP GET request
        public string Get(string url)
        {
            string jString;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.AutomaticDecompression = DecompressionMethods.GZip;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                jString = reader.ReadToEnd();
            }


            return jString;
        }



        
        // HTTP PUT request
        public string Put(string url, object msg)
        {
            return MethodHelper(url, msg, "PUT");
        }

        // HTTP POST request
        public string Post(string url, object msg)
        {
            return MethodHelper(url, msg, "POST");
        }

        // HTTP PATCH request
        public string Patch(string url, object msg)
        {
            return MethodHelper(url, msg, "PATCH");
        }


        // Helper method to eliminate reduntant code between methods like POST and PUT
        private string MethodHelper(string url, object msg, string method)
        {
            var json = JsonConvert.SerializeObject(msg);

            var request = WebRequest.CreateHttp(url);
            request.Method = method;
            request.ContentType = "application/json";
            var buffer = Encoding.UTF8.GetBytes(json);
            request.ContentLength = buffer.Length;
            request.GetRequestStream().Write(buffer, 0, buffer.Length);
            var response = request.GetResponse();
            json = new StreamReader(response.GetResponseStream()).ReadToEnd();

            return json;
        }


        // HTTP DELETE request
        // Param: grpName: The name of the group to DELETE
        // Param: user: Under what user is the group to DELETE
        public string Delete(string grpName, string user)
        {

            string url = Child(user).Child("groups").Child(grpName).Build();

            WebRequest request = WebRequest.Create(url);
            request.Method = "DELETE";

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            return response.ToString();
        }


        // Captures current open programs and stores their paths in firebase
        // Param: grpName: What the created group will be called
        // Param: user: Under what user should this group be created
        public void CaptureGroup(string grpName, string user)
        {
            Dictionary<string, string> pathsDict = Patherian.GetAllPaths();

            List<string> paths = new List<string>();

            foreach (KeyValuePair<string, string> pair in pathsDict)
            {
                paths.Add(pair.Value);
            }

            string url = Child(user).Child("groups").Child(grpName).Child("paths").Build();
            string urlDate = Child(user).Child("groups").Child(grpName).Child("dateadded").Build();
            string urlRecent = Child(user).Child("groups").Child(grpName).Child("dateopened").Build();

            Put(url, paths);
            Put(urlDate, DateTime.Now.ToString("G"));
            Put(urlRecent, DateTime.Now.ToString("G"));

        }


        // Opens all paths contained in a group
        // Param: grpName: The name of the group to be opened
        // Param: user: Under what user is this group
        public void OpenGroup(string grpName, string user)
        {
            string url = Child(user).Child("groups").Child(grpName).Child("paths").Build();
            string[] processes = JsonConvert.DeserializeObject<string[]>(Get(url));
            Patherian.OpenProcesses(processes);
            string urlRecent = Child(user).Child("groups").Child(grpName).Child("dateopened").Build();
            Put(urlRecent, DateTime.Now.ToString("G"));


        }

        // Get the names of all the groups under a user
        public Dictionary<string, PathGroup> GetGroupNames(string user)
        {
            string url = Child(user).Child("groups").Build();
            Dictionary<string, PathGroup> groups = JsonConvert.DeserializeObject<Dictionary<string, PathGroup>>(Get(url));

            

            return groups;
        }


    }
}

