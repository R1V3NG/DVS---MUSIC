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
        public string PlaylistTrackIndex { get; set; }
        public Songs(string playlist, string name, string directory, string playlistTrackIndex) 
        {
            this.playlist = playlist;
            this.name = name;
            this.directory = directory;
            this.PlaylistTrackIndex = playlistTrackIndex;
        }
    }
}
