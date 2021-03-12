using System;

public class Room 
{
    [Serializable]
    public class RoomOption
    {
        public string roomName;
        public string passward;
        public int maxPlayer;
        public string mapName;
    }

    public RoomOption roomOption;
}
