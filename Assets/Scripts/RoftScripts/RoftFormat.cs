using ROFTIOMANAGEMENT;

public class RoftFormat
{
    const string newLine = "\n";

    #region Format Version
    static string t_version = "Format Version".AsTag();
    static string p_formatVer = "1.06v" + newLine;
    #endregion

    #region [General]
    static string t_general = "General".AsTag();
    static string p_Author = "Author".AsProperty(System.Environment.UserName) + newLine;
    static string p_AudioFileName = "AudioFilename".AsProperty(RoftCreator.audioFilePath) + newLine;
    static string p_BackgroundImage = "BackgroundImage".AsProperty(RoftCreator.backgroundFilePath) + newLine;
    static string p_BackgroundVideo = "BackgroundVideo".AsProperty() + newLine;
    #endregion

    #region [Metadata]
    static string t_metadata = "Metadata".AsTag();
    static string p_Title = "Title".AsProperty(RoftCreator.GetSongTitle()) + newLine;
    static string p_TitleUnicode = "TitleUnicode".AsProperty(RoftCreator.GetSongTitle(true)) + newLine;
    static string p_Artist = "Artist".AsProperty(RoftCreator.GetSongArtist()) + newLine;
    static string p_ArtistUnicode = "ArtistUnicode".AsProperty(RoftCreator.GetSongArtist(true)) + newLine;
    static string p_Creator = "Creator".AsProperty(System.Environment.UserName) + newLine;
    static string p_ROFTID = "ROFTID".AsProperty(RoftCreator.GetROFTID()) + newLine;
    static string p_GROUPID = "GROUPID".AsProperty(RoftCreator.GetGROUPID()) + newLine;
    #endregion

    #region [Difficulty]
    static string t_difficulty = "Difficulty".AsTag();
    static string p_DifficultyName = "DifficultyName".AsProperty(RoftCreator.GetDifficultyName()) + newLine;
    static string p_StressBuild = "StressBuild".AsProperty(RoftCreator.GetStressBuild().ToString()) + newLine;
    static string p_ObjectCount = "ObjectCount".AsProperty(!ObjectLogger.IsNull() ? ObjectLogger.GetObjectCount() : 0) + newLine;
    #region Key Count
    static string keyInfo = GetLayoutType();


    #endregion
    static string p_KeyCount = "KeyLayout".AsProperty(keyInfo) + newLine;

    static string p_AccuracyHarshness = "AccuracyHarshness".AsProperty(RoftCreator.GetAccuracyHarshness()) + newLine;
    static string p_ApproachSpeed = "ApproachSpeed".AsProperty(RoftCreator.GetApproachSpeed()) + newLine;
    #endregion

    #region [Timing]
    static string t_timing = "Timing".AsTag();
    static string p_BPM = "BPM".AsProperty(ObjectLogger.Get_BPM()) + newLine;
    static string p_Offset = "Offset".AsProperty(ObjectLogger.Get_Offset()) + newLine;
    static string p_PreviewPosition = "PreviewPosition".AsProperty() + newLine;
    #endregion

    #region [Objects]
    static string t_objects = "Objects".AsTag();
    static string objectData { get; set; } = ObjectLogger.IsNull() == false ? ObjectLogger.ObjectData : "NIL";
    #endregion

    #region [Record]
    static string t_records = "Records".AsTag();
    #endregion

    #region .rftm Information
    static string[] rftmInformation = new string[]
    {
                   //Format Version
                   t_version +
                   p_formatVer,

                   //General
                   t_general +
                   p_Author +
                   p_AudioFileName +
                   p_BackgroundImage +
                   p_BackgroundVideo,

                   //Metadata
                   t_metadata +
                   p_Title +
                   p_TitleUnicode +
                   p_Artist +
                   p_ArtistUnicode +
                   p_Creator +
                   p_ROFTID +
                   p_GROUPID,

                   //Timing
                   t_timing +
                   p_BPM +
                   p_Offset,

                   //Difficulty
                   t_difficulty +
                   p_DifficultyName +
                   p_StressBuild +
                   p_ObjectCount +
                   p_KeyCount +
                   p_AccuracyHarshness +
                   p_ApproachSpeed,

                   //Objects
                   t_objects +
                   objectData,

                   //Record
                   t_records
    };
    #endregion


    RoftFormat()
    {
        #region Format Version
        t_version = "Format Version".AsTag();
        p_formatVer = "1.06v" + newLine;
        #endregion

        #region [General]
        t_general = "General".AsTag();
        p_Author = "Author".AsProperty(System.Environment.UserName) + newLine;
        p_AudioFileName = "AudioFilename".AsProperty(RoftCreator.audioFilePath) + newLine;
        p_BackgroundImage = "BackgroundImage".AsProperty(RoftCreator.backgroundFilePath) + newLine;
        p_BackgroundVideo = "BackgroundVideo".AsProperty() + newLine;
        #endregion

        #region [Metadata]
        t_metadata = "Metadata".AsTag();
        p_Title = "Title".AsProperty(RoftCreator.GetSongTitle()) + newLine;
        p_TitleUnicode = "TitleUnicode".AsProperty(RoftCreator.GetSongTitle(true)) + newLine;
        p_Artist = "Artist".AsProperty(RoftCreator.GetSongArtist()) + newLine;
        p_ArtistUnicode = "ArtistUnicode".AsProperty(RoftCreator.GetSongArtist(true)) + newLine;
        p_Creator = "Creator".AsProperty(System.Environment.UserName) + newLine;
        p_ROFTID = "ROFTID".AsProperty(RoftCreator.GetROFTID()) + newLine;
        p_GROUPID = "GROUPID".AsProperty(RoftCreator.GetGROUPID()) + newLine;
        #endregion

        #region [Difficulty]
        t_difficulty = "Difficulty".AsTag();
        p_DifficultyName = "DifficultyName".AsProperty(RoftCreator.GetDifficultyName()) + newLine;
        p_StressBuild = "StressBuild".AsProperty(RoftCreator.GetStressBuild().ToString()) + newLine;
        p_ObjectCount = "ObjectCount".AsProperty(!ObjectLogger.IsNull() ? ObjectLogger.GetObjectCount() : 0) + newLine;
        #region Key Count
        keyInfo = GetLayoutType();


        #endregion
        p_KeyCount = "KeyLayout".AsProperty(keyInfo) + newLine;

        p_AccuracyHarshness = "AccuracyHarshness".AsProperty(RoftCreator.GetAccuracyHarshness()) + newLine;
        p_ApproachSpeed = "ApproachSpeed".AsProperty(RoftCreator.GetApproachSpeed()) + newLine;
        #endregion

        #region [Timing]
        t_timing = "Timing".AsTag();
        p_BPM = "BPM".AsProperty(ObjectLogger.Get_BPM()) + newLine;
        p_Offset = "Offset".AsProperty(ObjectLogger.Get_Offset()) + newLine;
        p_PreviewPosition = "PreviewPosition".AsProperty() +newLine;
        #endregion

        #region [Objects]
        t_objects = "Objects".AsTag();
        objectData = ObjectLogger.IsNull() == false ? ObjectLogger.ObjectData : "NIL";
        #endregion

        #region [Record]
        t_records = "Records".AsTag();
        #endregion

        #region .rftm Information
        rftmInformation = new string[]
        {
                   //Format Version
                   t_version +
                   p_formatVer,

                   //General
                   t_general +
                   p_Author +
                   p_AudioFileName +
                   p_BackgroundImage +
                   p_BackgroundVideo,

                   //Metadata
                   t_metadata +
                   p_Title +
                   p_TitleUnicode +
                   p_Artist +
                   p_ArtistUnicode +
                   p_Creator +
                   p_ROFTID +
                   p_GROUPID,

                   //Timing
                   t_timing +
                   p_BPM +
                   p_Offset +
                   p_PreviewPosition,

                   //Difficulty
                   t_difficulty +
                   p_DifficultyName +
                   p_StressBuild +
                   p_ObjectCount +
                   p_KeyCount +
                   p_AccuracyHarshness +
                   p_ApproachSpeed,

                   //Objects
                   t_objects +
                   objectData,

                   //Record
                   t_records
        };
        #endregion
    }

    /// <summary>
    /// Get Layouttype used in generating format.
    /// </summary>
    /// <returns>Layout type, or how many keys are used.</returns>
    static string GetLayoutType() => RoftCreator.GetTotalKeys().ToString();

    /// <summary>
    /// Get the newly generated format as an array.
    /// </summary>
    /// <returns>An array of information that include tags and their properties.</returns>
    public string[] GetFormatInfo() => rftmInformation;

    /// <summary>
    /// Create new format with updated information.
    /// </summary>
    /// <returns></returns>
    public static RoftFormat New()
    {
        return new RoftFormat();
    }
}
