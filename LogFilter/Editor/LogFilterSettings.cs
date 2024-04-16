using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FFramework.FUnityEditor
{

    [FUnityEditor.FilePath("FFramework/Settings/LogFilterSettings.json", FUnityEditor.FilePathAttribute.Location.ProjectFolder)]
    public class LogFilterSettings :JsonSingleton<LogFilterSettings>
    {
        
        public List<string> CustomInfos = new List<string>();

        public List<string> BuiltInInfos = new List<string>();
        public bool ShowBuiltIn = false;

        
    }
}