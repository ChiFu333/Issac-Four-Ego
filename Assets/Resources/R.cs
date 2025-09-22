using UnityEngine;

public static partial class R
{
    public static bool isInited = false;
    public static VoiceSO normalVoice;

    public static void InitAll()
    {
        isInited = true;
        normalVoice = Resources.Load<VoiceSO>("TinyVoice");
        R.InitAudio();
    }
}
