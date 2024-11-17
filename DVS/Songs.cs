using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DVS
{
    public class Songs
    {
        string playlist {  get; set; }
        string name { get; set; }

        public Songs(string playlist, string name) 
        {
            this.playlist = playlist;
            this.name = name;
        }
    }
}
