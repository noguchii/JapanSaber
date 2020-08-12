using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JapanSaber.Search
{
    /// <summary>
    /// set 可能な IBeatmapLevelCollection 実装
    /// </summary>
    public class SimpleBeatmapLevelCollection : IBeatmapLevelCollection
    {
        public IPreviewBeatmapLevel[] beatmapLevels { get; set; }
            = new IPreviewBeatmapLevel[0];
    }
}
