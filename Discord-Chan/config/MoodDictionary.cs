﻿using Discord_Chan.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Discord_Chan.Config
{
    static class MoodDictionary
    {
        private static Dictionary<MoodHandleService.Mood, Dictionary<string, string>> moodDictionary = new Dictionary<MoodHandleService.Mood, Dictionary<string, string>>();
        
        public static void InitializeMoodDictionary()
        {
            foreach (MoodHandleService.Mood mood in Enum.GetValues(typeof(MoodHandleService.Mood)))
            {
                moodDictionary.Add(mood, JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText($"./mood/{mood}.json")));
            }
        }

        public static string getMoodMessage(string message)
        {
            return moodDictionary[MoodHandleService.getMood()][message];
        }
    }
}
