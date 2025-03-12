using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class R 
{
    public static class Audio
    {
        public static AudioClip playerTakeDamage;
        public static AudioClip playerDie;
        public static AudioClip enemyTakeDamage;

        public static AudioClip cardTaked;
        public static List<AudioClip> cardMouseOnLootInHands;

        public static AudioClip statUp;
        public static AudioClip heal;
    }

    public static class EffectSprites
    {
        public static Sprite heal;
        public static Sprite healthUp;
        public static Sprite attackUp;
        public static Sprite cubeUp;
    }
    public static void Init()
    {
        Audio.playerTakeDamage = Resources.Load<AudioClip>("Audio/" + "Hurt_grunt");
        Audio.enemyTakeDamage = Resources.Load<AudioClip>("Audio/" + "Cute_Grunt");
        Audio.playerDie = Resources.Load<AudioClip>("Audio/" + "Isaac_dies");
        
        Audio.cardTaked = Resources.Load<AudioClip>("Audio/" + "CardTaked");
        Audio.cardMouseOnLootInHands = new List<AudioClip>()
        {
            Resources.Load<AudioClip>("Audio/" + "MouseOnLootCard1"),
            Resources.Load<AudioClip>("Audio/" + "MouseOnLootCard2"),
            Resources.Load<AudioClip>("Audio/" + "MouseOnLootCard3"),
        };
        
        Audio.statUp = Resources.Load<AudioClip>("Audio/" + "1up");
        Audio.heal = Resources.Load<AudioClip>("Audio/" + "Vamp_Gulp");
            
        EffectSprites.heal = Resources.Load<Sprite>("PopUpSprites/" + "HealHp");
        EffectSprites.healthUp = Resources.Load<Sprite>("PopUpSprites/" + "HealthUp");
        EffectSprites.attackUp = Resources.Load<Sprite>("PopUpSprites/" + "AttackUp");
        EffectSprites.cubeUp = Resources.Load<Sprite>("PopUpSprites/" + "CubeUp");
    }
}
