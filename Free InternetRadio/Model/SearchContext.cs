using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iRadio_Free.Model
{
    public class SearchContext:PropertyChangedBase
    {
        public int CurrentPage { get; set; }
        private bool _IsMoreAvailable;
        public bool IsMoreAvailable
        {
            get { return _IsMoreAvailable; }
            set { _IsMoreAvailable = value; OnPropertyChanged("IsMoreAvailable"); }
        }
        public string SearchUrl { get; set; }
        public Mode SearchMode { get; set; }
        public Genre SearachGenre { get; set; }
        public string SearchKeyWord { get; set; }

        public SearchContext()
        {
            this.SearchMode = Mode.FeaturedPopular;
            CurrentPage = 1;
        }
        public SearchContext(Mode mode)
        {
            this.SearchMode = mode;
            CurrentPage = 1;
        }
        public SearchContext(string keyword)
        {
            this.SearchMode = Mode.Keyword;
            this.SearchKeyWord = keyword;
            CurrentPage = 1;
        }
        public SearchContext(Genre genre)
        {
            this.SearchMode = Mode.Genre;
            this.SearachGenre = genre;
            CurrentPage = 1;
        }

    }
}
