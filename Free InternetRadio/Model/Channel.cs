using HtmlAgilityPack;
using iRadio_Free.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Web.Http;
using Windows.Web.Http.Filters;

namespace iRadio_Free.Model
{
    public class Channel:PropertyChangedBase
    {
        [XmlIgnoreAttribute]
        public MainViewModel VMRef;
        public Channel()
        {
            Genres = new List<Genre>();
            PreviousChannel = null;
            NextChannel = null;
        }

        private Channel _PreviousChannel;
        public Channel PreviousChannel
        {
            get { return _PreviousChannel; }
            set { _PreviousChannel = value; OnPropertyChanged("PreviousChannel"); }
        }


        private Channel _NextChannel;
        public Channel NextChannel
        {
            get { return _NextChannel; }
            set { _NextChannel = value; OnPropertyChanged("NextChannel"); }
        }

        public string ChannelName { get; set; }
        public string ChannelLink { get; set; }

        private bool _IsUpdating;

        [XmlIgnoreAttribute]
        public bool IsUpdating
        {
            get { return _IsUpdating; }
            set { _IsUpdating = value; OnPropertyChanged("IsUpdating"); }
        }


        public string ChannelAddress { get; set; }

        public string Refferer { get; set; }
        private bool _IsPlaying;
        public bool IsPlaying
        {
            get { return _IsPlaying; }
            set { _IsPlaying = value; OnPropertyChanged(""); }
        }

        private bool _IsInFavorites;
        public bool IsInFavorites
        {
            get { return _IsInFavorites; }
            set { _IsInFavorites = value; OnPropertyChanged("IsInFavorites"); }
        }


        public List<Genre> Genres { get; set; }
        public string OfficialLink { get; set; }

        public string Quality { get; set; }
       


        private string _CurrentTrack;
        public string CurrentTrack
        {
            get { return _CurrentTrack; }
            set { _CurrentTrack = value; OnPropertyChanged("CurrentTrack"); }
        }

        public async void UpdateCurrentStatus()
        {
          
            if (IsUpdating) return;
            try
            {
                
                IsUpdating = true;
                bool resolved = false;
                try
                {
                    HttpBaseProtocolFilter bs = new HttpBaseProtocolFilter();
                    bs.CacheControl.ReadBehavior = HttpCacheReadBehavior.NoCache;
                    HttpClient client = new HttpClient(bs);
                    client.DefaultRequestHeaders["User-Agent"] = ConstStrings.UserAgent;
                    client.DefaultRequestHeaders["Referer"] = "https://www.internet-radio.com";
                    client.DefaultRequestHeaders["Host"] = "www.internet-radio.com";
                    client.DefaultRequestHeaders["Accept"] = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8";
                    string res = await (await client.GetAsync(new Uri(ChannelLink))).Content.ReadAsStringAsync();
                    HtmlDocument doc = new HtmlDocument();
                    doc.LoadHtml(res);
                    var decs = doc.DocumentNode.Descendants();
                    foreach (var dec in decs)
                    {
                        if (dec.InnerText == "Now Playing : " && dec.ParentNode.Name == "p" && dec.ParentNode.Attributes["class"]?.Value == "lead" && dec.NextSibling?.Name == "b")
                        {
                            CurrentTrack = dec.NextSibling.InnerText;
                            resolved = true;
                            break;
                        }
                    }
                }
                catch 
                {
                    
                }
                if (!resolved)
                {
                    var searchRes = await VMRef.SearchNameAsync(ChannelName);
                    foreach (var c in searchRes)
                    {
                        if (c.ChannelName == ChannelName )
                        {
                            CurrentTrack = c.CurrentTrack;
                        }
                    }
                }
                VMRef?.UpdateIfInFavorites(this);
            }
            catch
            {
               
            }
            finally
            {
                IsUpdating = false;
            }
        }


    }
}
