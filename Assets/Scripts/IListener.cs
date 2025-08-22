using UnityEngine;

public enum EVENT_TYPE
{
    EGameStart, //start the game
    EGameEnd,   //end the game
    EUseMove,   //player move
    EUseSkill,  //player use skill
}

public interface IListener
{
    void OnEvent(EVENT_TYPE eventType, Component sender, object param = null);
}
