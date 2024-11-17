using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DVS
{
    public class Playlist
    {
        public string name {  get; set; }
        public string author { get; set; }

        public Playlist(string name, string author)
        {
            this.name = name;
            this.author = author;
        }
    }
}
