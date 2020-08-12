using BS_Utils.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace JapanSaber.Objects
{
    /// <summary>
    /// set 可能な BeatmapData
    /// </summary>
    public static class SimpleBeatmapData
    {
        public static BeatmapData Create(int notesCount = 0,
            int bombsCount = 0,
            int obstaclesCount = 0,
            int spawnRotationEventsCount = 0)
        {
            var data = new BeatmapData(new BeatmapLineData[0], new BeatmapEventData[0]);
            data.GetType().GetProperty("notesCount", BindingFlags.Instance | BindingFlags.Public).SetValue(data, notesCount);
            data.GetType().GetProperty("bombsCount", BindingFlags.Instance | BindingFlags.Public).SetValue(data, bombsCount);
            data.GetType().GetProperty("obstaclesCount", BindingFlags.Instance | BindingFlags.Public).SetValue(data, obstaclesCount);
            data.GetType().GetProperty("spawnRotationEventsCount", BindingFlags.Instance | BindingFlags.Public).SetValue(data, spawnRotationEventsCount);

            return data;
        }
    }
}

