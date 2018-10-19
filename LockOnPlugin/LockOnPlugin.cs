﻿using System;
using System.ComponentModel;
using System.Reflection;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using BepInEx;
using ParadoxNotion.Serialization;

namespace LockOnPluginKK
{
    [BepInPlugin("keelhauled.lockonpluginkk", "LockOnPluginKK", "1.0.0")]
    public class LockOnPlugin : BaseUnityPlugin
    {
        [DisplayName("!Tracking speed")]
        [Description("The speed at which the target is followed.")]
        [AcceptableValueRange(0.01f, 0.3f, true)]
        public static ConfigWrapper<float> TrackingSpeedNormal { get; private set; }

        [DisplayName("Manage cursor visibility")]
        public static ConfigWrapper<bool> ManageCursorVisibility { get; private set; }

        [DisplayName("Scroll through males")]
        [Description("When switching characters with the hotkeys, only females are selected by default.")]
        public static ConfigWrapper<bool> ScrollThroughMalesToo { get; private set; }
        
        [DisplayName("Show messages")]
        public static ConfigWrapper<bool> ShowInfoMsg { get; private set; }

        [DisplayName("!Leash length")]
        [Description("The amount of slack allowed when tracking.")]
        [AcceptableValueRange(0f, 0.5f, true)]
        public static ConfigWrapper<float> LockLeashLength { get; private set; }

        [DisplayName("Auto lock on switch")]
        [Description("Lock on automatically after switching characters.")]
        public static ConfigWrapper<bool> AutoSwitchLock { get; private set; }

        [DisplayName("!Lock on")]
        public static SavedKeyboardShortcut LockOnKey { get; private set; }

        [DisplayName("!Show target gui")]
        public static SavedKeyboardShortcut LockOnGuiKey { get; private set; }

        [DisplayName("Select previous character")]
        public static SavedKeyboardShortcut PrevCharaKey { get; private set; }

        [DisplayName("Select next character")]
        public static SavedKeyboardShortcut NextCharaKey { get; private set; }

        public static TargetData targetData;
        string fileName = "LockOnPlugin.json";

        LockOnPlugin()
        {
            TrackingSpeedNormal = new ConfigWrapper<float>("LockedTrackingSpeed", this, 0.1f);
            ManageCursorVisibility = new ConfigWrapper<bool>("ManageCursorVisibility", this, false);
            ScrollThroughMalesToo = new ConfigWrapper<bool>("ScrollThroughMalesToo", this, true);
            ShowInfoMsg = new ConfigWrapper<bool>("ShowInfoMsg", this, true);
            LockLeashLength = new ConfigWrapper<float>("LockLeashLength", this, 0f);
            AutoSwitchLock = new ConfigWrapper<bool>("AutoSwitchLock", this, false);

            LockOnKey = new SavedKeyboardShortcut("LockOnKey", this, new KeyboardShortcut(KeyCode.N));
            LockOnGuiKey = new SavedKeyboardShortcut("LockOnGuiKey", this, new KeyboardShortcut(KeyCode.L));
            PrevCharaKey = new SavedKeyboardShortcut("PrevCharaKey", this, new KeyboardShortcut(KeyCode.None));
            NextCharaKey = new SavedKeyboardShortcut("NextCharaKey", this, new KeyboardShortcut(KeyCode.None));
        }

        void Awake()
        {
            string dataPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), fileName);

            if(File.Exists(dataPath))
            {
                try
                {
                    var data = File.ReadAllText(dataPath);
                    targetData = JSONSerializer.Deserialize<TargetData>(data);
                }
                catch(Exception)
                {
                    Console.WriteLine("Failed to deserialize target data. Loading backup.");
                    LoadResourceData();
                }
            }
            else
            {
                Console.WriteLine("Loading default target data.");
                LoadResourceData();
            }

            SceneLoaded();
            SceneManager.sceneLoaded += SceneLoaded;
        }

        void OnDestroy() // for ScriptEngine
        {
            SceneManager.sceneLoaded += SceneLoaded;
        }

        void LoadResourceData()
        {
            var resourceName = nameof(LockOnPluginKK) + fileName;
            using(Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                using(StreamReader reader = new StreamReader(stream))
                {
                    string result = reader.ReadToEnd();
                    targetData = JSONSerializer.Deserialize<TargetData>(result);
                }
            }
        }

        void SceneLoaded(Scene scene, LoadSceneMode mode)
        {
            SceneLoaded();
        }

        void SceneLoaded()
        {
            var comp = gameObject.GetComponent<LockOnBase>();

            if(FindObjectOfType<StudioScene>())
            {
                if(!comp) gameObject.AddComponent<StudioMono>();
            }
            else if(comp)
            {
                Destroy(comp);
            }
        }
    }
}