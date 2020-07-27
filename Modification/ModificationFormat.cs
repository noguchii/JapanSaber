using System;

namespace JapanSaber.Modification
{
    [Serializable]
    public enum ModificationFormat
    {
        Title_Subtitle_Author_Mapper = 0,
        Title_Author_Product_Mapper,
        Title_Subtitle_Product_Mapper,
        Title_Product_Author_Mapper,
        Title_Subtitle_Author_Product,
    }
}
