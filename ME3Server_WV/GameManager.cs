using System;
using System.Collections.Generic;

namespace ME3Server_WV
{
    public static class GameManager
    {
        public static List<GameInfo> AllGames = new List<GameInfo>();
        public class GameInfo
        {
            public struct Attribut
            {
                public string Name;
                public string Value;
            }
            public Player.PlayerInfo Creator;
            public List<Player.PlayerInfo> OtherPlayers;            
            public List<Attribut> Attributes;
            public Blaze.TdfDoubleList ATTR;
            public int GAMESETTING, GAMESTATE;
            public long ID;
            public long MID;
            public long GSID;
            public bool Update;
            public bool isActive;
            public string VSTR = "";
            public byte[] XNNC = null;
            public byte[] XSES = null;

            public static GameInfo CreateGame(Player.PlayerInfo player)
            {
                GameInfo res = new GameInfo();
                res.Creator = player;
                res.ID = (uint)(AllGames.Count + 0x5DC695);
                res.MID = (uint)(AllGames.Count + 0x1129DA20);
                res.isActive = true;
                res.Update = true;
                res.OtherPlayers = new List<Player.PlayerInfo>();
                res.Attributes = new List<Attribut>();
                res.GAMESETTING = 0;
                res.GAMESTATE = 0;
                res.GSID = 0;
                AllGames.Add(res);
                return res;
            }

            public void MakeATTR()
            {
                List<string> keys = new List<string>();
                List<string> data = new List<string>();
                foreach (Attribut a in Attributes)
                {
                    keys.Add(a.Name);
                    data.Add(a.Value);
                }
                ATTR = Blaze.TdfDoubleList.Create("ATTR", 1, 1, keys, data, Attributes.Count);
            }

            public void UpdateGameSetting(int setting)
            {
                GAMESETTING = setting;
                Update = true;
            }

            public void UpdateGameState(int state)
            {
                GAMESTATE = state;
                Update = true;
            }

            public void UpdateAttributes(string[] names, string[] values)
            {
                for (int i = 0; i < names.Length; i++)
                {
                    int idx = FindOrCreate(names[i]);
                    if (idx != -1)
                    {
                        Attribut a = Attributes[idx];
                        a.Value = values[i];
                        Attributes[idx] = a;
                    }
                    else
                    {
                        Attribut a = new Attribut();
                        a.Name = names[i];
                        a.Value = values[i];
                        Attributes.Add(a);
                    }
                }
                Update = true;
            }
            public int FindOrCreate(string name)
            {
                for (int i = 0; i < Attributes.Count; i++)
                    if (Attributes[i].Name == name)
                        return i;
                return -1;
            }

            public string GetAttrValue(string name)
            {
                for (int i = 0; i < Attributes.Count; i++)
                    if (Attributes[i].Name == name)
                        return Attributes[i].Value;
                return "";
            }

            public List<Player.PlayerInfo> AllPlayers
            {
                get
                {
                    List<Player.PlayerInfo> piList = new List<Player.PlayerInfo>();
                    if(Creator.isActive)
                        piList.Add(Creator);
                    piList.AddRange(OtherPlayers);
                    return piList;
                }
            }
        }

        public static GameInfo FindByGID(long GID)
        {
            foreach (GameInfo g in AllGames)
                if (g.ID == GID)
                    return g;
            return null;
        }

        public static GameInfo FindFirstActive()
        {
            Logger.Log("Finding games ... ", LogColor.Blue);
            foreach (GameInfo g in AllGames) {
                Logger.Log("[Game] " + g.isActive + " - " + g.ID + " - " + g.OtherPlayers.Count, LogColor.Cyan);
                if (g.isActive && g.OtherPlayers.Count < 3)
                    return g;
            }
            return null;
        }

        /// <summary>
        /// Finds the active game that a player is currently in (as creator or other player).
        /// </summary>
        public static GameInfo FindByPlayer(Player.PlayerInfo player)
        {
            for (int i = AllGames.Count - 1; i >= 0; i--)
            {
                GameInfo g = AllGames[i];
                if (!g.isActive)
                    continue;
                if (g.Creator != null && g.Creator.PlayerID == player.PlayerID)
                    return g;
                foreach (Player.PlayerInfo pl in g.OtherPlayers)
                    if (pl.PlayerID == player.PlayerID)
                        return g;
            }
            return null;
        }

    }
}
