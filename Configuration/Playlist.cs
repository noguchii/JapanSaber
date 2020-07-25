using System;
using UnityEngine;

namespace JapanSaber.Configuration
{
    [Serializable]
    public class Playlist
    {
        [SerializeField]
        public SongInfos[] songs;
        [SerializeField]
        public string playlistTitle;
        [SerializeField]
        public string playlistAuthor;
        [SerializeField]
        public string image;
    }
}
