using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DVS
{
    public class Songs
    {
        public string playlist {  get; set; }
        public string name { get; set; }
        public string directory { get; set; }
        public Songs(string playlist, string name, string directory) 
        {
            this.playlist = playlist;
            this.name = name;
            this.directory = directory;
        }
    }
}
