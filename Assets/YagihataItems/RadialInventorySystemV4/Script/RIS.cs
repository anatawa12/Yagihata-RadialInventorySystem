﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace YagihataItems.RadialInventorySystemV4
{
    public static class RIS
    {
        public const string WorkFolderPath = "Assets/YagihataItems/RadialInventorySystemV4/";
        public const string AutoGeneratedFolderPath = WorkFolderPath + "AutoGenerated/";
        public const string CurrentVersion = "ris_v4.0_tesing";
        public const string SettingsNameV3 = "RISV3Settings";
        public const string SettingsNameV4 = "RISV4Settings";
        public const string VersionUrl = "https://www.negura-karasu.net/radialinventory/vercheck/";
        public const string DonatorListUrl = "https://www.negura-karasu.net/radialinventory/donators/";
        public const string ManualUrl = "https://www.negura-karasu.net/archives/1456";
        public const string TwitterUrl = "https://twitter.com/Yagihata4x";
        public const string DownloadUrl = "https://yagihata.booth.pm/items/2278448";
        public const VRCExpressionParameters.ValueType IntParam = VRCExpressionParameters.ValueType.Int;
        public const VRCExpressionParameters.ValueType BoolParam = VRCExpressionParameters.ValueType.Bool;
        public readonly static GUIStyle CountBarStyleL = new GUIStyle(GUI.skin.label) { fontSize = 10, alignment = TextAnchor.UpperLeft, margin = new RectOffset(10, 10, 0, 0) };
        public readonly static GUIStyle CountBarStyleR = new GUIStyle(GUI.skin.label) { fontSize = 10, alignment = TextAnchor.UpperRight, margin = new RectOffset(10, 10, 0, 0) };
        public readonly static GUIStyle DonatorLabelStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.UpperCenter, margin = new RectOffset(10, 10, 20, 20), wordWrap = true };
        public enum RISMode
        {
            Simple,
            Basic,
            Advanced
        }
        public enum PropGroup
        {
            None,
            Group1,
            Group2,
            Group3,
            Group4,
            Group5,
            Group6,
            Group7,
            Group8,
        }
        public enum BoneType
        {
            None,
            Head
        }
    }
}
