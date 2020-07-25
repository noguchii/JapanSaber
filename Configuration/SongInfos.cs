using System;
using UnityEngine;

namespace JapanSaber.Configuration
{
    [Serializable]
    public class SongInfos
    {
        /// <summary>
        /// SongPlaylistLoaderが小文字を認識し、シリアライズ属性でフィールド名を指定できないため
        /// プロパティ名が小文字になっている
        /// </summary>
        [SerializeField]
        public string hash;
        [SerializeField]
        public string MapKey;
        [SerializeField]
        public string Title;
        [SerializeField]
        public string Subtitle;
        [SerializeField]
        public string Author;
        [SerializeField]
        public string Product;
        [SerializeField]
        public string Yomi;
    }
}