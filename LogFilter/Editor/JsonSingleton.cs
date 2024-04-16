using System;
using System.IO;
using System.Reflection;
using UnityEditor;

namespace FFramework.FUnityEditor
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class FilePathAttribute : Attribute
    {
        public enum Location
        {
            ProjectFolder,
            AssetsFolder,
            AbsoluteFoder,
            PerferenceFolder
        }

        string relative;

        /// <summary>
        /// 相对于Project的路径
        /// </summary>
        public string Path => relative;

        public static string GetRelativePath(string basePath, string targetPath)
        {
            // 将路径转化为绝对路径
            basePath = System.IO.Path.GetFullPath(basePath);
            targetPath = System.IO.Path.GetFullPath(targetPath);

            // 将路径分割成目录数组
            string[] baseDirs = basePath.Split(System.IO.Path.DirectorySeparatorChar);
            string[] targetDirs = targetPath.Split(System.IO.Path.DirectorySeparatorChar);

            // 查找公共目录的索引
            int commonIndex = 0;
            while (commonIndex < baseDirs.Length && commonIndex < targetDirs.Length &&
                   baseDirs[commonIndex] == targetDirs[commonIndex])
            {
                commonIndex++;
            }

            // 计算要跳出多少层目录
            int levelsToGoUp = baseDirs.Length - commonIndex;

            // 构建相对路径
            string relativePath = string.Empty;
            for (int i = 0; i < levelsToGoUp; i++)
            {
                relativePath += System.IO.Path.DirectorySeparatorChar;
            }

            // 将目标路径的剩余部分追加到相对路径中
            for (int i = commonIndex; i < targetDirs.Length; i++)
            {
                relativePath += targetDirs[i] + System.IO.Path.DirectorySeparatorChar;
            }

            // 去除最后的目录分隔符
            if (relativePath.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString()))
            {
                relativePath = relativePath.Substring(0, relativePath.Length - 1);
            }

            return relativePath;
        }

        /// <summary>
        /// 绝对路径
        /// </summary>
        public string Abs => System.IO.Path.Combine(EditorHelper.ProjectPath, relative);

        public FilePathAttribute(string relativePath, Location location)
        {
            switch (location)
            {
                case Location.ProjectFolder:
                    relative = relativePath;
                    break;
                case Location.AssetsFolder:
                    relative = "Assets/" + relativePath;
                    break;
                case Location.AbsoluteFoder:
                    relative = GetRelativePath(EditorHelper.ProjectPath, relativePath);
                    break;
                case Location.PerferenceFolder:
                    relative = GetRelativePath(System.IO.Path.Combine(EditorHelper.ProjectPath, "UserSettings/"), relativePath);
                    break;

            }
        }
    }

    /// <summary>
    /// 以Json形式存储的单例
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class JsonSingleton<T> where T : JsonSingleton<T>, new()
    {
        private static T _ins = null;
        public static T instance
        {
            get
            {
                //空
                if (_ins == null)
                {
                    //检查特性
                    FilePathAttribute att = typeof(T).GetCustomAttribute<FilePathAttribute>();
                    _ins = new T();
                    //有特性
                    if (att != null&&File.Exists(att.Abs)) 
                    {
                        
                        //读取并覆盖
                        using (StreamReader reader = new StreamReader(att.Abs))
                        {
                            string json = reader.ReadToEnd();
                            EditorJsonUtility.FromJsonOverwrite(json,_ins);
                        }
                    }
                }
                return _ins;

            }
        }

        public void Save()
        {
            //转json
            string json = EditorJsonUtility.ToJson(this, true);
            //查特性
            FilePathAttribute att = typeof(T).GetCustomAttribute<FilePathAttribute>();
            //无特性抛异常
            if (att == null)
            {
                throw new System.Exception($"{typeof(T).FullName} need {typeof(FilePathAttribute).FullName}");
            }
            else
            {
                EditorHelper.NotExistCreate(att.Abs);
                //有则保存
                using (StreamWriter writer = new StreamWriter(att.Abs, false, System.Text.Encoding.UTF8))
                {
                    writer.Write(json);
                }
            }
        }
    }
}