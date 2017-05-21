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
            this.Show();
            
            
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
                var r = response.Content;
                r = r.Substring(1);
                r = r.Substring(0, r.Length - 1);
                Console.Write(r);
                var obj = JObject.Parse(r);
                foreach(var child in obj.Children())
                {
                    
                    var list = child.ToList();
                    for (int i = 0; i < list.Count(); i++) {
                        string val = list.ElementAt(i).ToString();
                        
                    }
                }
            }
        }
    }
}
