
using HtmlAgilityPack;
using iRadio_Free.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.System;
using Windows.UI.Popups;
using Windows.Web.Http;
using Windows.Web.Http.Filters;

namespace iRadio_Free.ViewModel
{
    public class MainViewModel : PropertyChangedBase
    {
        private bool _IsSearching;
        public bool IsSearching
        {
            get { return _IsSearching; }
            set { _IsSearching = value; OnPropertyChanged("IsSearching"); }
        }

        public ObservableCollection<Genre> Genres { get; set; }
        public ObservableCollection<Channel> Results { get; set; }
        public SearchContext context { get; set; }
        ApplicationDataContainer localsettings { get; set; }
        ApplicationDataContainer favorites { get; set; }
        private bool isRated;
      
        public MainViewModel()
        {
        

            context = new SearchContext();
            Results = new ObservableCollection<Channel>();
            var genres = Enum.GetValues(typeof(Genre));
            Genres = new ObservableCollection<Genre>();
            foreach (var g in genres)
            {
                Genres.Add((Genre)g);
            }
            try
            {
                localsettings = ApplicationData.Current.LocalSettings;

               
                if (localsettings.Containers.ContainsKey("Favorites"))
                {
                    favorites = localsettings.Containers["Favorites"];
                }
                else
                {
                    favorites = localsettings.CreateContainer("Favorites", ApplicationDataCreateDisposition.Always);
                }
                isRated = true;
                object item = localsettings.Values["isRated"];
                string str = "NotRated";
                if (item != null)
                {
                    str = (string)item;
                }
                if (str == "NotRated")
                {
                    this.isRated = false;
                }
                if (str == "Rated")
                {
                    this.isRated = true;
                }
            }
            catch (Exception)
            {
                
            }
        }
        public async Task ShowRateDialog()
        {
            try
            {
                if (isRated == false)
                {
                    var RateCounterObject = localsettings.Values["RateCounter"];
                    int RateCounter = 0;
                    if (RateCounterObject != null)
                    {
                        RateCounter = (int)RateCounterObject;
                    }
                    if (RateCounter >= 15)
                    {
                        localsettings.Values["RateCounter"] = 0;
                        MessageDialog RatingDialog = new MessageDialog("If you like our app please rate or review it and help us provide a better service.");
                        RatingDialog.Commands.Add(new UICommand("Rate now", (arg) =>
                        {
                            localsettings.Values["isRated"] = "Rated";
                            isRated = true;
                            Launcher.LaunchUriAsync(new Uri("ms-windows-store:REVIEW?PFN=" + Package.Current.Id.FamilyName));
                        }));
                        RatingDialog.Commands.Add(new UICommand("Not now", (arg) =>
                        {

                        }));
                        await RatingDialog.ShowAsync();
                    }
                    else
                    {
                        RateCounter += 1;
                        localsettings.Values["RateCounter"] = RateCounter;
                    }
                }

            }
            catch (Exception)
            {

            }
        }
        public async Task<string> SerializeToString(Channel channel)
        {
            bool oldval = channel.IsPlaying;
            channel.IsPlaying = false;
            var nextchannel = channel.NextChannel;
            var prevchannel = channel.PreviousChannel;
            channel.NextChannel = null;
            channel.PreviousChannel = null;
            XmlSerializer xmlSerializer = new XmlSerializer(channel.GetType());
            using (StringWriter textWriter = new StringWriter())
            {
                xmlSerializer.Serialize(textWriter, channel);
                channel.NextChannel = nextchannel;
                channel.PreviousChannel = prevchannel;
                channel.IsPlaying = oldval;
                return textWriter.ToString();
            }


        }
        public async Task<Channel> DeserializeToChannel(string data)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(Channel));
            using (MemoryStream ms = new MemoryStream())
            {
                byte[] bdata = Encoding.Unicode.GetBytes(data);
                ms.Write(bdata, 0, bdata.Length);
                ms.Position = 0;

                var res = xmlSerializer.Deserialize(ms);
                return res as Channel;
            }
        }
        public async Task AddToFavorite(Channel channel)
        {
            string channeldata = await SerializeToString(channel);
            favorites.Values[channel.ChannelAddress] = channeldata;
            await CheckFavoriteExist(channel);
        }
        public async Task UpdateIfInFavorites(Channel channel)
        {
            try
            {
                if(favorites.Values.ContainsKey(channel.ChannelAddress))
                {
                    string channeldata = await SerializeToString(channel);
                    favorites.Values[channel.ChannelAddress] = channeldata;
                }
            }
            catch
            {
            }
        }

        public async Task RemoveFromFavorites(Channel channel)
        {
            if(favorites.Values.ContainsKey(channel.ChannelAddress))
            {
                favorites.Values.Remove(channel.ChannelAddress);                
            }
            await CheckFavoriteExist(channel);
        }
        async Task<List<Channel>> LoadFavorites()
        {
            List<Channel> res = new List<Channel>();
            foreach(var data in favorites.Values)
            {
                try
                {
                    Channel chanel = await DeserializeToChannel((string)data.Value);
                    chanel.IsInFavorites = true;
                    if(res.Count>0)
                    {
                        res.Last().NextChannel = chanel;
                        chanel.PreviousChannel = res.Last();
                    }
                    chanel.VMRef = this;
                    res.Add(chanel);
                }
                catch
                {

                }
            }
            return res;
        }
        async Task CheckFavoriteExist(Channel channel)
        {
            bool res = false;
            if(favorites.Values.ContainsKey(channel.ChannelAddress))
            {
                res = true;
            }
            else
            {
                res = false;
            }
            channel.IsInFavorites = res;
        }
        async Task<string> GetHTML(string url, string referrer)
        {
            return await Task.Run(async () =>
           {
               HttpBaseProtocolFilter bs = new HttpBaseProtocolFilter();
               bs.CacheControl.ReadBehavior = HttpCacheReadBehavior.NoCache;
               HttpClient client = new HttpClient(bs);
               client.DefaultRequestHeaders["User-Agent"] = ConstStrings.UserAgent;
               client.DefaultRequestHeaders["Accept"] = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
               client.DefaultRequestHeaders["Accept-Encoding"] = "gzip, deflate, sdch";
               client.DefaultRequestHeaders["Host"] = "www.internet-radio.com";
               if (referrer.Length != 0)
               {
                   client.DefaultRequestHeaders["Referer"] = "http://www.internet-radio.com/search/?";
               }
               return await (await client.GetAsync(new Uri(url))).Content.ReadAsStringAsync();
           });
        }


        public async Task<List<Channel>> SearchNameAsync(string keyword)
        {
            return await Task.Run(async () =>
            {
                var retvalue = new List<Channel>();
                string url = url = "http://www.internet-radio.com/search/?radio=" + keyword.Replace(" ", "+");

                string html = await GetHTML(url, "");
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(html);
                var tables = from table in doc.DocumentNode.Descendants("table")
                             where table.Attributes["class"]?.Value == "table table-striped"
                             select table;
                foreach (var table in tables)
                {
                    var res = await ParseChannelsFromNode(table);
                    retvalue.AddRange(res);
                }

                var linodes = from ulnode in doc.DocumentNode.Descendants("ul")
                              where ulnode.Attributes["class"]?.Value == "pagination"
                              from linode in ulnode.Descendants("li")
                              select linode;

                foreach (var linode in linodes)
                {
                    if (linode.Attributes["class"]?.Value == "next")
                    {
                        context.IsMoreAvailable = true;
                        context.CurrentPage++;
                        break;
                    }
                }


                return retvalue;
            });
        }

            public async Task<List<Channel>> SearchAsync()
        {
            return await Task.Run(async () =>
            {
                context.IsMoreAvailable = false;
                List<Channel> retvalue = new List<Channel>();
                string url = "";
                switch (context.SearchMode)
                {
                    case Mode.FeaturedPopular:
                        {
                            url = "http://www.internet-radio.com/";
                            break;
                        }
                    case Mode.Genre:
                        {
                            if (context.CurrentPage <= 1)
                            {
                                url = "http://www.internet-radio.com/stations/" + context.SearachGenre.ToString().Replace("DigitStart", "").Replace("_", "%20") + "/";
                            }
                            else
                            {
                                url = "http://www.internet-radio.com/stations/" + context.SearachGenre.ToString().Replace("DigitStart", "").Replace("_", "%20") + "/page" + context.CurrentPage.ToString();
                            }
                            break;
                        }
                    case Mode.Keyword:
                        {
                            if (context.CurrentPage <= 1)
                            {
                                url = "http://www.internet-radio.com/search/?radio=" + context.SearchKeyWord.Replace(" ", "+");
                            }
                            else
                            {
                                url = "http://www.internet-radio.com/search/?radio=" + context.SearchKeyWord.Replace(" ", "+") + "&page=/page" + context.CurrentPage.ToString();
                            }
                            break;
                        }
                }              

                switch (context.SearchMode)
                {
                    case Mode.Favorites:
                        {
                            var res = await LoadFavorites();
                            retvalue.AddRange(res);
                            context.IsMoreAvailable = false;
                            break;
                        }
                    case Mode.FeaturedPopular:
                        {
                            string html = await GetHTML(url, "");
                            HtmlDocument doc = new HtmlDocument();
                            doc.LoadHtml(html);
                            var h2nodes = doc.DocumentNode.Descendants("h2");
                            HtmlNode featurednode = null;
                            HtmlNode popularnode = null;
                            foreach (var h2node in h2nodes)
                            {
                                if (featurednode == null && h2node.InnerText.Contains("Featured Radio Stations") && h2node.ParentNode.Name == "div" && h2node.ParentNode.Attributes["class"]?.Value == "col-md-6")
                                {
                                    featurednode = h2node.ParentNode;
                                }
                                if (popularnode == null && h2node.InnerText.Contains("Popular Radio Stations") && h2node.ParentNode.Name == "div" && h2node.ParentNode.Attributes["class"]?.Value == "col-md-6")
                                {
                                    popularnode = h2node.ParentNode;
                                }
                                if (popularnode != null && featurednode != null) break;
                            }
                            if (popularnode != null)
                            {
                                var popularchanels = await ParseChannelsFromNode(popularnode);
                                if(retvalue.Count>0&&popularchanels.Count>0)
                                {
                                    var lastretch = retvalue.Last();
                                    var firstpopchan = popularchanels.First();
                                    lastretch.NextChannel = firstpopchan;
                                    firstpopchan.PreviousChannel = lastretch;

                                }
                                retvalue.AddRange(popularchanels);
                            }
                            if (featurednode != null)
                            {
                                var FeaturedChanels = await ParseChannelsFromNode(featurednode);


                                if (retvalue.Count > 0 && FeaturedChanels.Count > 0)
                                {
                                    var lastretch = retvalue.Last();
                                    var firstfeatchan = FeaturedChanels.First();
                                    lastretch.NextChannel = firstfeatchan;
                                    firstfeatchan.PreviousChannel = lastretch;

                                }


                                retvalue.AddRange(FeaturedChanels);
                            }



                            break;
                        }
                    default:
                        {
                            string html = await GetHTML(url, "");
                            HtmlDocument doc = new HtmlDocument();
                            doc.LoadHtml(html);
                            var tables = from table in doc.DocumentNode.Descendants("table")
                                         where table.Attributes["class"]?.Value == "table table-striped"
                                         select table;
                            foreach (var table in tables)
                            {
                                var res = await ParseChannelsFromNode(table);
                                retvalue.AddRange(res);
                            }

                            var linodes = from ulnode in doc.DocumentNode.Descendants("ul")
                                          where ulnode.Attributes["class"]?.Value == "pagination"
                                          from linode in ulnode.Descendants("li")
                                          select linode;

                            foreach (var linode in linodes)
                            {
                                if (linode.Attributes["class"]?.Value == "next")
                                {
                                    context.IsMoreAvailable = true;
                                    context.CurrentPage++;
                                    break;
                                }
                            }

                            break;
                        }
                }
                try
                {
                    foreach(Channel ch in retvalue)
                    {
                        try
                        {
                            await CheckFavoriteExist(ch);
                        }
                        catch (Exception)
                        {
                            
                        }
                    }
                }
                catch
                {

                }

                return retvalue;

            });
        }
        async Task<List<Channel>> ParseChannelsFromNode(HtmlNode node)
        {
            List<Channel> channels = new List<Channel>();
            try
            {
                var trnodes = node.Descendants("tr");
                Channel PreviousChannel = null;
                foreach (var trnode in trnodes)
                {
                    try
                    {
                        var trdecents = trnode.Descendants();
                        Channel channel = new Channel() { VMRef = this };

                        foreach (var trd in trdecents)
                        {
                            if (trd.Name == "i" && trd.Attributes["class"]?.Value == "jp-play text-danger mdi-av-play-circle-outline" && trd.Attributes.Contains("onclick"))
                            {
                                try
                                {
                                    var m = Regex.Match(trd.Attributes["onclick"].Value, @"http://.*.pls");
                                    if (m.Success)
                                    {
                                        int lastindex = m.Value.LastIndexOf("/");
                                        if (lastindex != -1)
                                        {
                                            channel.ChannelAddress = new string(m.Value.Take(lastindex).ToArray());
                                        }

                                    }
                                    else
                                    {
                                        m = Regex.Match(trd.Attributes["onclick"].Value, @"http://.*.m3u");
                                        if (m.Success)
                                        {

                                            channel.ChannelAddress = m.Value.Replace(".m3u", "");
                                            

                                        }
                                    }
                                   

                                  


                                }
                                catch (Exception ee)
                                {

                                }

                            }
                            if (trd.Name == "b" && trd.ParentNode.Name == "td")
                            {
                                channel.CurrentTrack = trd.InnerText;
                            }
                            if (trd.Name == "h4" && trd.Attributes["class"]?.Value == "text-danger" && trd.ParentNode.Name == "td")
                            {
                                channel.ChannelName = trd.InnerText;
                                var anodes = trd.Descendants("a");
                                try
                                {
                                    if (anodes.Count() == 1)
                                    {
                                        var anode = anodes.First();
                                        if (anode.Attributes["href"]?.Value.StartsWith("/station/") ?? false)
                                        {
                                            channel.ChannelLink = "https://www.internet-radio.com" + anode.Attributes["href"].Value;
                                        }
                                    }
                                }
                                catch
                                {
                                    
                                }
                            }
                            if (trd.Name == "#text" && trd.InnerText == "Genres: ")
                            {
                                var currentsibling = trd.NextSibling;
                                while (currentsibling != null && currentsibling.Name == "a")
                                {
                                    string genre = currentsibling.InnerText;
                                    if (genre.Length > 0 && char.IsDigit(genre[0]))
                                    {
                                        genre = string.Concat("DigitStart", genre);
                                    }

                                    Genre g;

                                    bool parsed = Enum.TryParse(genre.Replace(" ", "_"), out g);
                                    if (parsed) channel.Genres.Add(g);

                                    if (currentsibling.NextSibling != null)
                                    {
                                        currentsibling = currentsibling.NextSibling.NextSibling;
                                    }
                                }

                            }

                        }
                        if (channel.ChannelAddress?.Length > 0)
                        {
                            if (PreviousChannel != null)
                            {
                                channel.PreviousChannel = PreviousChannel;
                                PreviousChannel.NextChannel = channel;
                            }
                            PreviousChannel = channel;
                            channels.Add(channel);
                        }
                        else
                        {

                        }
                    }
                    catch (Exception ee)
                    {

                    }
                }
            }
            catch
            {

            }
            return channels;

        }
    }
}
