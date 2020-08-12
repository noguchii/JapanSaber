using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace JapanSaber.Objects
{
    /// <summary>
    /// set 可能な BeatmapCharateristicSO
    /// </summary>
    public class SimpleBeatmapCharacteristicSO : BeatmapCharacteristicSO
    {
        public SimpleBeatmapCharacteristicSO(string serializedName = "", 
            string compoundIdPartName = "", 
            string characteristicNameLocalizationKey = "",
            string descriptionLocalizationKey = "",
            int sortingOrder = 0,
            bool containsRotationEvents = false,
            bool requires360Movement = false,
            Sprite icon = null) 
        {
            this._compoundIdPartName = compoundIdPartName;
            this._serializedName = serializedName;
            this._containsRotationEvents = containsRotationEvents;
            this._characteristicNameLocalizationKey = characteristicNameLocalizationKey;
            this._sortingOrder = sortingOrder;
            this._descriptionLocalizationKey = descriptionLocalizationKey;
            this._requires360Movement = requires360Movement;
            this._icon = icon;
        }
    }
}
