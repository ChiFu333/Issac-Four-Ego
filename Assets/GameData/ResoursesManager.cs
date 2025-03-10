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
    }
    public static void Init()
    {
        Audio.playerTakeDamage = Resources.Load<AudioClip>("Audio/" + "Hurt_grunt");
        Audio.enemyTakeDamage = Resources.Load<AudioClip>("Audio/" + "Cute_Grunt");
        Audio.playerDie = Resources.Load<AudioClip>("Audio/" + "Isaac_dies");
        Audio.cardTaked = Resources.Load<AudioClip>("Audio/" + "CardTaked");
    }
}
