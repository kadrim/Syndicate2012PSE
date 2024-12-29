using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualBasic;

namespace ME3Server_WV
{

    public class ME3MP_Profile
    {
        // REQUIRED
        public string[] Lines;
        public ME3PlayerHeader Header;
        public ME3PlayerBase Base;
        public ME3PlayerClass[] Classes = new ME3PlayerClass[6];
        public List<ME3PlayerChar> Chars;
        // OPTIONAL
        public ME3PlayerFaceCodes FaceCodes;
        public ME3PlayerNewItem NewItem;
        public ME3PlayerBanner Banner;
        public ME3PlayerChallengeStats ChallengeStats;

        public static ME3MP_Profile InitializeFromFile(string Filename)
        {
            var profile = new ME3MP_Profile();
            try
            {
                // System.Diagnostics.Debug.Print("ME3MP_Profile.InitializeFromFile / Load file")
                profile.Lines = System.IO.File.ReadAllLines(Filename);
                // Header
                // System.Diagnostics.Debug.Print("ME3MP_Profile.InitializeFromFile / Header")
                profile.Header = new ME3PlayerHeader(profile.Lines);
                // Base
                // System.Diagnostics.Debug.Print("ME3MP_Profile.InitializeFromFile / Base")
                int x = profile.SeekLine("Base=");
                profile.Base = new ME3PlayerBase(profile.Lines[x].Substring(5));
                // Class
                // System.Diagnostics.Debug.Print("ME3MP_Profile.InitializeFromFile / Class")
                int I;
                for (I = 1; I <= 6; I++)
                {
                    x = profile.SeekLine("class" + I + "=");
                    profile.Classes[I - 1] = new ME3PlayerClass(profile.Lines[x].Substring(7));
                }
                // Chars
                // System.Diagnostics.Debug.Print("ME3MP_Profile.InitializeFromFile / Chars")
                profile.Chars = new List<ME3PlayerChar>();
                I = 0; // start at 0
                do
                {
                    string strchar = "char" + I + "=";
                    x = profile.SeekLine(strchar);
                    if (x == -1)
                        break;
                    // add char to profile main collection
                    profile.Chars.Add(new ME3PlayerChar(profile.Lines[x].Substring(strchar.Length)));
                    // add char to its respective class collection
                    AddCharToClass(profile, profile.Chars[I].KitClassName, I);
                    I += 1;
                }
                while (true);
                // start of optional stuff
                // FaceCodes
                // System.Diagnostics.Debug.Print("ME3MP_Profile.InitializeFromFile / FaceCodes")
                x = profile.SeekLine("FaceCodes=");
                if (x != -1)
                    profile.FaceCodes = new ME3PlayerFaceCodes(profile.Lines[x]);
                // NewItem
                // System.Diagnostics.Debug.Print("ME3MP_Profile.InitializeFromFile / NewItem")
                x = profile.SeekLine("NewItem=");
                if (x != -1)
                    profile.NewItem = new ME3PlayerNewItem(profile.Lines[x]);
                // Banner
                // System.Diagnostics.Debug.Print("ME3MP_Profile.InitializeFromFile / Banner")
                x = profile.SeekLine("csreward=");
                if (x != -1)
                    profile.Banner = new ME3PlayerBanner(profile.Lines[x]);
                // Challenge stats
                // System.Diagnostics.Debug.Print("ME3MP_Profile.InitializeFromFile / ChallengeStats")
                var CSparam = new string[6];
                x = profile.SeekLine("Completion=");
                if (x != -1)
                    CSparam[0] = profile.Lines[x].Substring(11);
                x = profile.SeekLine("Progress=");
                if (x != -1)
                    CSparam[1] = profile.Lines[x].Substring(9);
                x = profile.SeekLine("cscompletion=");
                if (x != -1)
                    CSparam[2] = profile.Lines[x].Substring(13);
                x = profile.SeekLine("cstimestamps=");
                if (x != -1)
                    CSparam[3] = profile.Lines[x].Substring(13);
                x = profile.SeekLine("cstimestamps2=");
                if (x != -1)
                    CSparam[4] = profile.Lines[x].Substring(14);
                x = profile.SeekLine("cstimestamps3=");
                if (x != -1)
                    CSparam[5] = profile.Lines[x].Substring(14);
                profile.ChallengeStats = new ME3PlayerChallengeStats(CSparam[0], CSparam[1], CSparam[2], CSparam[3], CSparam[4], CSparam[5]);
                // System.Diagnostics.Debug.Print("ME3MP_Profile.InitializeFromFile / Done")
                return profile;
            }
            catch (Exception ex)
            {
                Debug.Print("ME3MP_Profile.InitializeFromFile / " + ex.GetType().Name + " / " + ex.Message);
                return null;
            }
        }

        private static void AddCharToClass(ME3MP_Profile profile, string ClassName, int CharIndex)
        {
            switch (ClassName ?? "")
            {
                case ME3PlayerClass.CLASS1:
                    {
                        profile.Classes[0].Members.Add(CharIndex);
                        break;
                    }
                case ME3PlayerClass.CLASS2:
                    {
                        profile.Classes[1].Members.Add(CharIndex);
                        break;
                    }
                case ME3PlayerClass.CLASS3:
                    {
                        profile.Classes[2].Members.Add(CharIndex);
                        break;
                    }
                case ME3PlayerClass.CLASS4:
                    {
                        profile.Classes[3].Members.Add(CharIndex);
                        break;
                    }
                case ME3PlayerClass.CLASS5:
                    {
                        profile.Classes[4].Members.Add(CharIndex);
                        break;
                    }
                case ME3PlayerClass.CLASS6:
                    {
                        profile.Classes[5].Members.Add(CharIndex);
                        break;
                    }
            }
        }

        #region Shortcut / Helper Functions
        public int GetN7Rating()
        {
            int TotalLevel = 0;
            for (int I = 0; I <= 5; I++)
            {
                if (IsClassActive(I))
                    TotalLevel += Classes[I].GetLevel();
            }
            return GetTotalPromotions() * 30 + TotalLevel;  // 30 -> 20 from leveling the class, plus 10 from bonus for promoting
        }

        public int GetTotalPromotions()
        {
            int TotalPromotions = 0;
            for (int I = 0; I <= 5; I++)
                TotalPromotions += Classes[I].GetPromotions();
            return TotalPromotions;
        }

        public int GetChallengePoints()
        {
            return ChallengeStats.ChallengePoints;
        }

        public string GetPlayerName()
        {
            return Header.GetDisplayName();
        }

        public long GetPlayerID()
        {
            return Header.GetPID();
        }

        public string GetPassword()
        {
            return Header.GetAuth2();
        }
        #endregion

        public bool IsClassActive(int ClassNumber)
        {
            foreach (int M in Classes[ClassNumber].Members)
            {
                if (Chars[M].GetDeployed())
                    return true;
            }
            return false;
        }

        public int SeekLine(string str)
        {
            for (int I = 0, loopTo = Lines.Count() - 1; I <= loopTo; I++)
            {
                if (Lines[I].StartsWith(str))
                    return I;
            }
            return -1;
        }

        public bool SaveToFile(string Filename)
        {
            try
            {
                System.IO.File.WriteAllLines(Filename, ToLines());
                return true;
            }
            catch (Exception ex)
            {
                Debug.Print("ME3MP_Profile.SaveToFile / " + ex.GetType().Name + " / " + ex.Message);
                return false;
            }
        }

        public List<string> ToLines(bool IncludeHeader = true)
        {
            var listLines = new List<string>();
            // Header : PID UID AUTH AUTH2 DSNM
            if (IncludeHeader)
                listLines.AddRange(Header.GetLines());
            // Base
            listLines.Add("Base=" + Base.ToString());
            // Classes
            foreach (ME3PlayerClass c in Classes)
                listLines.Add(c.ToString());
            // Chars
            for (int I = 0, loopTo = Chars.Count - 1; I <= loopTo; I++)
                listLines.Add("char" + I + "=" + Chars[I].ToString());
            // FaceCodes
            if (!(FaceCodes is null))
                listLines.Add(FaceCodes.GetLine());
            // NewItem
            if (!(NewItem is null))
                listLines.Add(NewItem.GetLine());
            // Banner
            if (!(Banner is null))
                listLines.Add(Banner.GetLine());
            // Challenge Stats
            listLines.AddRange(ChallengeStats.GetLines());
            return listLines;
        }

    }

    public class ME3PlayerBase
    {
        public const int INVENTORYITEMLASTINDEX = 670;

        private int[] SubValues = new int[10];
        // 0: unknown
        // 1: unknown
        // 2: credits
        // 3: unknown
        // 4: unknown
        // 5: credits spent
        // 6: unknown
        // 7: games played (finished)
        // 8: seconds played
        // 9: unknown
        private byte[] Items = new byte[671]; // SubValues(10)

        public ME3PlayerBase(string BaseValue)
        {
            string[] BaseFields;
            BaseFields = BaseValue.Split(char.Parse(";"));
            if (BaseFields.Count() != 11)
                throw new ArgumentException("BaseValue string must be 11 fields.");
            int ExpectedCharCount = (INVENTORYITEMLASTINDEX + 1) * 2;
            if (BaseFields[10].Length != ExpectedCharCount)
                throw new ArgumentException("Last field of BaseValue must be " + ExpectedCharCount + " chars (" + (INVENTORYITEMLASTINDEX + 1) + " inventory items).");
            for (int I = 0; I <= 9; I++)
                SubValues[I] = int.Parse(BaseFields[I]);
            StripIntoItemValues(BaseFields[10]);
        }

        private void StripIntoItemValues(string BigString)
        {
            for (int I = 0; I <= INVENTORYITEMLASTINDEX; I++)
                // I think I can do this shit in one line...
                Items[I] = byte.Parse(BigString.Substring(I * 2, 2), System.Globalization.NumberStyles.HexNumber);
        }

        public byte GetItem(int Index)
        {
            return Items[Index];
        }
        public void SetItem(int Index, byte Value)
        {
            Items[Index] = Value;
        }

        public int GetCredits()
        {
            return SubValues[2];
        }
        public void SetCredits(int intCredits)
        {
            SubValues[2] = intCredits;
        }

        public int GetGamesPlayed()
        {
            return SubValues[7];
        }
        public void SetGamesPlayed(int intGamesPlayed)
        {
            SubValues[7] = intGamesPlayed;
        }

        public int GetTimePlayedSeconds()
        {
            return SubValues[8];
        }
        public void SetTimePlayed(int intSeconds)
        {
            SubValues[8] = intSeconds;
        }
        public void SetTimePlayed(int intMinutes, int intSeconds)
        {
            SetTimePlayed(60 * intMinutes + intSeconds);
        }
        public void SetTimePlayed(int intHours, int intMinutes, int intSeconds)
        {
            SetTimePlayed(3600 * intHours + 60 * intMinutes + intSeconds);
        }

        public override string ToString()
        {
            var sb = new System.Text.StringBuilder();
            for (int I = 0; I <= 9; I++)
            {
                sb.Append(SubValues[I]);
                sb.Append(";");
            }
            sb.Append(GetItemsString());
            return sb.ToString();
        }

        private string GetItemsString()
        {
            var sb = new System.Text.StringBuilder();
            for (int I = 0; I <= INVENTORYITEMLASTINDEX; I++)
                sb.Append(Items[I].ToString("X2").ToLower());
            return sb.ToString();
        }

    }

    public class ME3PlayerClass
    {
        public const string CLASS1 = "Adept";
        public const string CLASS2 = "Soldier";
        public const string CLASS3 = "Engineer";
        public const string CLASS4 = "Sentinel";
        public const string CLASS5 = "Infiltrator";
        public const string CLASS6 = "Vanguard";

        private int field1_Version1;
        private int field2_Version2;
        private string field3_Name;
        private int field4_Level;
        private float field5_Exp;
        private int field6_Promotions;

        public List<int> Members;

        public ME3PlayerClass(string ClassValue)
        {
            string[] ClassFields = Strings.Split(ClassValue, ";");
            if (ClassFields.Count() != 6)
                throw new ArgumentException("ClassValue string must be 6 fields.");
            field1_Version1 = int.Parse(ClassFields[0]);
            field2_Version2 = int.Parse(ClassFields[1]);
            field3_Name = ClassFields[2];
            field4_Level = int.Parse(ClassFields[3]);
            field5_Exp = float.Parse(ClassFields[4], System.Globalization.CultureInfo.InvariantCulture);
            field6_Promotions = int.Parse(ClassFields[5]);
            Members = new List<int>();
        }

        /// <summary>
        /// Returns the full class line, including its proper number according to the class name. Ex: class1=20;4;Adept;20;10500000.0000;10
        /// </summary>
        public override string ToString()
        {
            // Return the whole damn line, ME3 puts classes in specific order
            string strResult = "";
            switch (Name ?? "")
            {
                case CLASS1:
                    {
                        strResult = "class1=";
                        break;
                    }
                case CLASS2:
                    {
                        strResult = "class2=";
                        break;
                    }
                case CLASS3:
                    {
                        strResult = "class3=";
                        break;
                    }
                case CLASS4:
                    {
                        strResult = "class4=";
                        break;
                    }
                case CLASS5:
                    {
                        strResult = "class5=";
                        break;
                    }
                case CLASS6:
                    {
                        strResult = "class6=";
                        break;
                    }
            }
            strResult += field1_Version1 + ";" + field2_Version2 + ";" + field3_Name + ";" + field4_Level + ";" + field5_Exp.ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture) + ";" + field6_Promotions;
            return strResult;
        }

        public string Name
        {
            get
            {
                return field3_Name;
            }
        }

        public int GetLevel()
        {
            return field4_Level;
        }
        public void SetLevel(int intLevel)
        {
            field4_Level = intLevel;
        }

        public float GetExperience()
        {
            return field5_Exp;
        }
        public void SetExperience(float sngExp)
        {
            field5_Exp = sngExp;
        }

        public int GetPromotions()
        {
            return field6_Promotions;
        }
        public void SetPromotions(int intPromotions)
        {
            field6_Promotions = intPromotions;
        }

    }

    public class ME3PlayerChar
    {
        // 00 Integer Version1 - should always be 20
        // 01 Integer Version2 - should always be 4
        // 02 String KitName - internal kit name as seen in Coalesced
        // 03 String CharacterName - the name given by the player
        // 04 Integer Tint1ID
        // 05 Integer Tint2ID
        // 06 Integer PatternID
        // 07 Integer PatternColorID
        // 08 Integer PhongID
        // 09 Integer EmissiveID
        // 10 Integer SkinToneID
        // 11 Integer SecondsPlayed - all time related variables seem to be unused
        // 12 Integer TimeStampYear
        // 13 Integer TimeStampMonth
        // 14 Integer TimeStampDay
        // 15 Integer TimeStampSeconds
        // 16 String Powers - not a simple string...
        // 17 String HotKeys - confirmed to be completely unused
        // 18 String Weapons - currently equipped weapons
        // 19 String WeaponMods - all weapons ever equipped and their respective equipped mods
        // 20 Boolean Deployed - char has been customized at least once
        // 21 Boolean LeveledUp - makes the 'level up' arrow appear
        private string[] fields = new string[22];

        public ME3PlayerChar(string strvalue)
        {
            string[] CharSplit = Strings.Split(strvalue, ";");
            fields = CharSplit;
        }

        public bool GetDeployed()
        {
            return bool.Parse(fields[20]);
        }
        public void SetDeployed(bool bValue)
        {
            fields[20] = bValue.ToString();
        }

        public string KitClassName
        {
            get
            {
                if (fields[2].Contains(ME3PlayerClass.CLASS1))
                    return ME3PlayerClass.CLASS1;
                if (fields[2].Contains(ME3PlayerClass.CLASS2))
                    return ME3PlayerClass.CLASS2;
                if (fields[2].Contains(ME3PlayerClass.CLASS3))
                    return ME3PlayerClass.CLASS3;
                if (fields[2].Contains(ME3PlayerClass.CLASS4))
                    return ME3PlayerClass.CLASS4;
                if (fields[2].Contains(ME3PlayerClass.CLASS5))
                    return ME3PlayerClass.CLASS5;
                return ME3PlayerClass.CLASS6;
            }
        }

        public override string ToString()
        {
            var sb = new System.Text.StringBuilder();
            for (int I = 0; I <= 20; I++)
                sb.Append(fields[I] + ";");
            sb.Append(fields[21]);
            return sb.ToString();
        }

    }

    public class ME3PlayerCharPower
    {
    }

    public class ME3PlayerHeader
    {
        private long PID;
        private long UID;
        private string Auth;
        private string Auth2;
        private string DSNM;

        public ME3PlayerHeader(string[] TextLines)
        {
            string[] s;
            s = TextLines[0].Split('=');
            if (s.Length == 2 && s[0] == "PID")
                PID = long.Parse(s[1].Substring(2), System.Globalization.NumberStyles.HexNumber);
            s = TextLines[1].Split('=');
            if (s.Length == 2 && s[0] == "UID")
                UID = long.Parse(s[1].Substring(2), System.Globalization.NumberStyles.HexNumber);
            s = TextLines[2].Split('=');
            if (s.Length == 2 && s[0] == "AUTH")
                Auth = s[1];
            s = TextLines[3].Split('=');
            if (s.Length == 2 && s[0] == "AUTH2")
                Auth2 = s[1];
            s = TextLines[4].Split('=');
            if (s.Length == 2 && s[0] == "DSNM")
                DSNM = s[1];
        }

        public long GetPID()
        {
            return PID;
        }
        public void SetPID(long Value)
        {
            PID = Value;
        }

        public long GetUID()
        {
            return UID;
        }
        public void SetUID(long Value)
        {
            UID = Value;
        }

        public string GetAuth()
        {
            return Auth;
        }
        public void SetAuth(string newAuth)
        {
            Auth = newAuth;
        }

        public string GetAuth2()
        {
            return Auth2;
        }
        public void SetAuth2(string newAuth2)
        {
            Auth2 = newAuth2;
        }

        public string GetDisplayName()
        {
            return DSNM;
        }
        public void SetDisplayName(string Name)
        {
            DSNM = Name;
        }

        public string[] GetLines()
        {
            string[] s = new string[] { "PID=0x" + PID.ToString("X8"), "UID=0x" + UID.ToString("X8"), "AUTH=" + Auth, "AUTH2=" + Auth2, "DSNM=" + DSNM };
            return s;
        }
    }

    public class ME3PlayerFaceCodes // WIP
    {
        private string facecodesline;
        public ME3PlayerFaceCodes(string lineFC)
        {
            facecodesline = lineFC;
        }
        public string GetLine()
        {
            return facecodesline;
        }
    }

    public class ME3PlayerNewItem // WIP
    {
        private string newitemline;
        public ME3PlayerNewItem(string lineNI)
        {
            newitemline = lineNI;
        }
        public string GetLine()
        {
            return newitemline;
        }
    }

    public class ME3PlayerBanner
    {
        private int bannerID;

        public ME3PlayerBanner(string lineBanner)
        {
            string[] s = Strings.Split(lineBanner, "=");
            bannerID = int.Parse(s[1]);
        }

        public void SetBannerID(int newID)
        {
            bannerID = newID;
        }

        public int GetBannerID()
        {
            return bannerID;
        }

        public void ResetBannerID()
        {
            SetBannerID(0);
        }

        public string GetLine()
        {
            return "csreward=" + bannerID;
        }

    }

    public class ME3PlayerChallengeStats
    {
        private List<int> completion_list;
        private List<int> progress_list;
        private List<int> cscompletion_list;
        private List<int> cstimestamps_list;
        private List<int> cstimestamps2_list;
        private List<int> cstimestamps3_list;

        public ME3PlayerChallengeStats(string completion_Str, string progress_Str, string cscompletion_Str, string cstimestamps_Str, string cstimestamps2_Str, string cstimestamps3_Str)
        {
            if (!string.IsNullOrEmpty(completion_Str))
                completion_list = StringToIntegerList(completion_Str);
            if (!string.IsNullOrEmpty(progress_Str))
                progress_list = StringToIntegerList(progress_Str);
            if (!string.IsNullOrEmpty(cscompletion_Str))
                cscompletion_list = StringToIntegerList(cscompletion_Str);
            if (!string.IsNullOrEmpty(cstimestamps_Str))
                cstimestamps_list = StringToIntegerList(cstimestamps_Str);
            if (!string.IsNullOrEmpty(cstimestamps2_Str))
                cstimestamps2_list = StringToIntegerList(cstimestamps2_Str);
            if (!string.IsNullOrEmpty(cstimestamps3_Str))
                cstimestamps3_list = StringToIntegerList(cstimestamps3_Str);
        }

        public List<string> GetLines()
        {
            var s = new List<string>();
            if (!(completion_list is null))
                s.Add("Completion=" + IntegerListToString(completion_list));
            if (!(progress_list is null))
                s.Add("Progress=" + IntegerListToString(progress_list));
            if (!(cscompletion_list is null))
                s.Add("cscompletion=" + IntegerListToString(cscompletion_list));
            if (!(cstimestamps_list is null))
                s.Add("cstimestamps=" + IntegerListToString(cstimestamps_list));
            if (!(cstimestamps2_list is null))
                s.Add("cstimestamps2=" + IntegerListToString(cstimestamps2_list));
            if (!(cstimestamps3_list is null))
                s.Add("cstimestamps3=" + IntegerListToString(cstimestamps3_list));
            return s;
        }

        public static string IntegerListToString(List<int> srcList)
        {
            var sb = new System.Text.StringBuilder();
            sb.Append(srcList[0]);
            for (int I = 1, loopTo = srcList.Count - 1; I <= loopTo; I += 1)
                sb.Append("," + srcList[I]);
            return sb.ToString();
        }

        public static List<int> StringToIntegerList(string srcStr)
        {
            string[] arrayStr = Strings.Split(srcStr, ",");
            var intList = new List<int>();
            foreach (string s in arrayStr)
                intList.Add(int.Parse(s));
            return intList;
        }

        public int ChallengePoints
        {
            get
            {
                if (completion_list is null)
                    return 0;
                return completion_list[1];
            }
        }

    }

}