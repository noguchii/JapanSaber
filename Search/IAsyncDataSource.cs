using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JapanSaber.Search
{
    /// <summary>
    /// AnnotatedBeatmapLevelsCollection の非同期読み込みが終わったかどうかの確認に使う
    /// </summary>
    public interface IAsyncDataSource
    {
        bool IsInitialLoading { get; }
        bool IsInitialLoaded { get; }
        void GetDataAsync();
        bool IsRefreshedVisualAfterInitialLoad { get; set; }
    }

    public interface IHaveDownladableLevel
    {
    }
}
