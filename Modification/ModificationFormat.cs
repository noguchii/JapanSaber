using System;

namespace JapanSaber.Modification
{
    [Serializable]
    public enum ModificationFormat
    {
        TitleSubtitleAuthorMapper = 0,
        TitleAuthorProductMapper,
        TitleSubtitleProductMapper,
        TitleProductAuthorMapper,
        TitleSubtitleAuthorProduct,
    }
}
