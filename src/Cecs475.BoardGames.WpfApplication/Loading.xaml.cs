using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using RestSharp;
using Newtonsoft.Json.Linq;
using System.Net;
using Newtonsoft.Json;
using System.Threading;

namespace Cecs475.BoardGames.WpfApplication
{
    /// <summary>
    /// Interaction logic for Loading.xaml
    /// </summary>
    public partial class Loading : Window
    {
        public Loading()
        {
            
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(WindowLoad);
            
            
            
        }
        public string cleanse(string str)
        {
            str = str.Trim();
            str = str.Replace("\r", "");
            str = str.Replace("\n", "");
            str = str.Replace(@"\", "");
            str = str.Replace("\"", "");
            
            return str;
        }
        public async void WindowLoad(object sender, RoutedEventArgs x)
        {
            var client = new RestClient("http://cecs475-boardgames.azurewebsites.net/");
            var request = new RestRequest("api/games", Method.GET);
            var task = client.ExecuteTaskAsync(request);
            var response = await task;
            if(response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                MessageBox.Show("Not found");
            }
            else
            {
                //request = new RestRequest("api/register", Method.POST);
                string r = response.Content;
                r = r.Substring(1);
                r = r.Substring(0, r.Length - 1);
                var obj = JObject.Parse(r);
                JToken results = obj.First;
                results = obj.Last.First;
                List<string> strList = new List<string>();
                List<JToken> things = new List<JToken>() { results.First, results.Last };
                foreach(JToken t in things)
                {
                    JToken token = t.First;
                    do
                    {
                        strList.Add(token.First.ToObject<string>());
                        token = token.Next;
                    }
                    while (token != null);
                }
                for (int i = 0; i < strList.Count(); i++)
                {
                    string val = strList.ElementAt(i);
                    if (val.Contains("https"))
                    {
                        var web = new WebClient();
                        var mTask = web.DownloadFileTaskAsync(val, "lib/"+strList.ElementAt(i-1));
                        
                        await mTask;
                        
                    }
                }
            }
            var win = new GameChoiceWindow();
            this.Close();
            win.Show();
            
        }

    }

    
}
