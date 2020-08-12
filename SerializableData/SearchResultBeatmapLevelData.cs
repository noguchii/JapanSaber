using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;

using JapanSaber.Objects;

namespace JapanSaber.SerializableData
{
    /// <summary>
    /// JapanSaberの検索結果のアイテム
    /// </summary>
    [JsonObject]
    public class SearchResultBeatmapLevelData : SongNames
    {
        static readonly AudioClip SheredEmptyAudioClip = AudioClip.Create("SheredEmptyAudioClip", 1, 1, 44100, false);

       // const int FLAGS_LEVELS_NONE = 0b000000000000;
        const int FLAGS_LEVELS_EASY = 0b000000000001;
        const int FLAGS_LEVELS_NORMAL = 0b000000000010;
        const int FLAGS_LEVELS_HARD = 0b000000000100;
        const int FLAGS_LEVELS_EXPERT = 0b000000001000;
        const int FLAGS_LEVELS_EXPERTPLUS = 0b000000010000;
        const int FLAGS_LEVELS_STANDARD = 0b000000100000;
        const int FLAGS_LEVELS_90DEGREE = 0b000001000000;
        const int FLAGS_LEVELS_360DEGREE = 0b000010000000;
        const int FLAGS_LEVELS_ONESABER = 0b000100000000;
        const int FLAGS_LEVELS_NOARROW = 0b001000000000;
        const int FLAGS_LEVELS_LIGHTSHOW = 0b010000000000;
        const int FLAGS_LEVELS_LAWLESS = 0b100000000000;
        const int FLAGS_LEVELS_ALLDIFFICULTY = 0b000000011111;
        const int FLAGS_LEVELS_ALLMODE = 0b111111100000;
       // const int FLAGS_LEVELS_ALL = 0b111111111111;

        [JsonIgnore]
        private static readonly Dictionary<int, BeatmapCharacteristicSO> CharacteristicSOsDictionary
            = new Dictionary<int, BeatmapCharacteristicSO>();

        [JsonIgnore]
        private static BeatmapCharacteristicCollectionSO _BeatmapCharacteristicCollectionSO;

        [JsonIgnore]
        public static BeatmapCharacteristicCollectionSO BeatmapCharacteristicCollectionSO
        {
            get
            {
                if (_BeatmapCharacteristicCollectionSO == null)
                {
                    _BeatmapCharacteristicCollectionSO = Resources.FindObjectsOfTypeAll<BeatmapCharacteristicCollectionSO>().First();
                }
                return _BeatmapCharacteristicCollectionSO;
            }
        }

        [JsonProperty]
        public int Length;
        [JsonProperty]
        public string LevelsList;
        [JsonProperty]
        public float Bpm;
        [JsonProperty]
        public float UpVotes;
        [JsonProperty]
        public float DownVotes;
        [JsonProperty]
        public float Rating;
        [JsonIgnore]
        public AudioClip AudioClip => SheredEmptyAudioClip;
        [JsonIgnore]
        public PreviewDifficultyBeatmapSet[] PreviewDifficultyBeatmapSets { get; set; }
        [JsonIgnore]
        public IDifficultyBeatmapSet[] DifficultyBeatmapSets { get; set; }

        public void Init(IBeatmapLevel rootBeatmap)
        {
            // LevelsList のデータ構成
            // 0,1,2,3,4|0,1,2,3,4|0,1,2,3,4|0,1,2,3,4

            // {index} : {data}
            // 0 : モードと難易度を組み合わせたフラグ
            // 1 : ノーツ数
            // 2 : ボム数
            // 3 : ウォール数
            // 4 : NJS

            //Logger.Debug(LevelsList);
            DifficultyBeatmapSets = LevelsList.Split('|')
                .Select(level => level.Split(','))
                .GroupBy(data => int.Parse(data[0]) & FLAGS_LEVELS_ALLMODE)  // モードごとに処理
                .Select<IGrouping<int, string[]>, IDifficultyBeatmapSet>(grouping =>
                {
                    var charateristic = GetBeatmapCharacteristicCollectionFromSO(ToSerializedName(grouping.Key));

                    if (charateristic == null)
                    {
                        if (CharacteristicSOsDictionary.ContainsKey(grouping.Key))
                        {
                            charateristic = CharacteristicSOsDictionary[grouping.Key];
                        }
                        else
                        {
                            charateristic = new SimpleBeatmapCharacteristicSO(
                               serializedName: ToSerializedName(grouping.Key),
                               compoundIdPartName: ToCompoundIdPartName(grouping.Key),
                               characteristicNameLocalizationKey: ToCharacteristicNameLocalizationKey(grouping.Key),
                               descriptionLocalizationKey: ToDescriptionLocalizationKey(grouping.Key),
                               sortingOrder: grouping.Key,
                               containsRotationEvents: ToContainsRotationEvents(grouping.Key),
                               requires360Movement: ToRequires360Movement(grouping.Key))
                            {
                                hideFlags = HideFlags.DontSave | HideFlags.NotEditable,
                            };
                            CharacteristicSOsDictionary.Add(grouping.Key, charateristic);
                        }
                    }

                    var difficultyBeatmapSet = new CustomDifficultyBeatmapSet(charateristic);
                    difficultyBeatmapSet.SetCustomDifficultyBeatmaps(
                         grouping.Select(data => new CustomDifficultyBeatmap(
                         rootBeatmap,                                   // level
                         difficultyBeatmapSet,                          // parent difficulty set
                         ToBeatmapDifficulty(int.Parse(data[0])),       // difficulty
                         (int)ToBeatmapDifficulty(int.Parse(data[0])),  // difficulty rank
                         float.Parse(data[4]),                          // note jump speed
                         0,                                             // start offset
                         SimpleBeatmapData.Create(
                             notesCount: int.Parse(data[1]),
                             bombsCount: int.Parse(data[2]),
                             obstaclesCount: int.Parse(data[3]),
                             spawnRotationEventsCount: 0)
                    )).OrderBy(x => x.difficultyRank).ToArray());
                    return difficultyBeatmapSet;
                })
                .ToArray();


            PreviewDifficultyBeatmapSets = LevelsList.Split('|')
                .Select(level => level.Split(','))
                .GroupBy(data => int.Parse(data[0]) & FLAGS_LEVELS_ALLMODE) // モードごとに処理
                .SelectMany(group =>
                group.Select(data => new PreviewDifficultyBeatmapSet(
                    GetBeatmapCharacteristicCollectionFromSO(ToSerializedName(group.Key)) ?? CharacteristicSOsDictionary[group.Key],
                    group
                    .Select(d => ToBeatmapDifficulty(int.Parse(d[0])))
                    .OrderBy(x => (int)x)
                    .ToArray()
                    )).ToArray())
                .ToArray();
        }

        public BeatmapCharacteristicSO GetBeatmapCharacteristicCollectionFromSO(string serializedName)
        {
            return BeatmapCharacteristicCollectionSO.GetBeatmapCharacteristicBySerializedName(serializedName);
        }

        public bool ToContainsRotationEvents(int flagValue)
        {
            switch (flagValue & FLAGS_LEVELS_ALLMODE)
            {
                case FLAGS_LEVELS_360DEGREE: return true;
                case FLAGS_LEVELS_90DEGREE: return true;
                case FLAGS_LEVELS_LIGHTSHOW:
                case FLAGS_LEVELS_ONESABER:
                case FLAGS_LEVELS_NOARROW:
                case FLAGS_LEVELS_LAWLESS:
                case FLAGS_LEVELS_STANDARD:
                default:
                    return false;
            }
        }
        public bool ToRequires360Movement(int flagValue)
        {
            switch (flagValue & FLAGS_LEVELS_ALLMODE)
            {
                case FLAGS_LEVELS_360DEGREE: return true;
                case FLAGS_LEVELS_90DEGREE:
                case FLAGS_LEVELS_LIGHTSHOW:
                case FLAGS_LEVELS_ONESABER:
                case FLAGS_LEVELS_NOARROW:
                case FLAGS_LEVELS_LAWLESS:
                case FLAGS_LEVELS_STANDARD:
                default:
                    return false;
            }
        }
        public BeatmapDifficulty ToBeatmapDifficulty(int flagValue)
        {
            switch (flagValue & FLAGS_LEVELS_ALLDIFFICULTY)
            {
                case FLAGS_LEVELS_EASY: return BeatmapDifficulty.Easy;
                case FLAGS_LEVELS_NORMAL: return BeatmapDifficulty.Normal;
                case FLAGS_LEVELS_HARD: return BeatmapDifficulty.Hard;
                case FLAGS_LEVELS_EXPERT: return BeatmapDifficulty.Expert;
                case FLAGS_LEVELS_EXPERTPLUS: return BeatmapDifficulty.ExpertPlus;
                default: return BeatmapDifficulty.Normal;
            }
        }
        public string ToSerializedName(int flagValue)
        {
            switch (flagValue & FLAGS_LEVELS_ALLMODE)
            {
                case FLAGS_LEVELS_360DEGREE: return "360Degree";
                case FLAGS_LEVELS_90DEGREE: return "90Degree";
                case FLAGS_LEVELS_LIGHTSHOW: return "Lightshow";
                case FLAGS_LEVELS_ONESABER: return "OneSaber";
                case FLAGS_LEVELS_NOARROW: return "NoArrows";
                case FLAGS_LEVELS_LAWLESS: return "Lawless";
                case FLAGS_LEVELS_STANDARD:
                default:
                    return "Standard";
            }
        }
        public string ToCompoundIdPartName(int flagValue)
        {
            switch (flagValue & FLAGS_LEVELS_ALLMODE)
            {
                case FLAGS_LEVELS_360DEGREE: return "360Degree";
                case FLAGS_LEVELS_90DEGREE: return "90Degree";
                case FLAGS_LEVELS_LIGHTSHOW: return "Lightshow";
                case FLAGS_LEVELS_ONESABER: return "OneSaber";
                case FLAGS_LEVELS_NOARROW: return "NoArrows";
                case FLAGS_LEVELS_LAWLESS: return "Lawless";
                case FLAGS_LEVELS_STANDARD:
                default:
                    return "";
            }
        }
        public string ToCharacteristicNameLocalizationKey(int flagValue)
        {
            switch (flagValue & FLAGS_LEVELS_ALLMODE)
            {
                case FLAGS_LEVELS_360DEGREE: return "LEVEL_360DEGREE";
                case FLAGS_LEVELS_90DEGREE: return "LEVEL_90DEGREE";
                case FLAGS_LEVELS_LIGHTSHOW: return "Lightshow";
                case FLAGS_LEVELS_ONESABER: return "LEVEL_ONE_SABER";
                case FLAGS_LEVELS_NOARROW: return "LEVEL_NO_ARROWS";
                case FLAGS_LEVELS_LAWLESS: return "Lawless";
                case FLAGS_LEVELS_STANDARD:
                default:
                    return "LEVEL_STANDARD";
            }
        }
        public string ToDescriptionLocalizationKey(int flagValue)
        {
            switch (flagValue & FLAGS_LEVELS_ALLMODE)
            {
                case FLAGS_LEVELS_360DEGREE: return "LEVEL_360DEGREE_HINT";
                case FLAGS_LEVELS_90DEGREE: return "LEVEL_90DEGREE_HINT";
                case FLAGS_LEVELS_LIGHTSHOW: return "Lightshow";
                case FLAGS_LEVELS_ONESABER: return "LEVEL_ONE_SABER";
                case FLAGS_LEVELS_NOARROW: return "LEVEL_NO_ARROWS_HINT";
                case FLAGS_LEVELS_LAWLESS: return "Lawless";
                case FLAGS_LEVELS_STANDARD:
                default:
                    return "LEVEL_STANDARD_HINT";
            }
        }
    }
}