using System;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JapanSaber
{
    internal static class Logger
    {
        internal static IPA.Logging.Logger IPALogger { get; set; }

        [Conditional("DEBUG")]
        internal static void Debug(string message)
        {
            IPALogger?.Debug(message);
        }

        [Conditional("DEBUG")]
        internal static void Debug(Exception ex)
        {
            IPALogger?.Debug(ex);
        }

        [Conditional("DEBUG")]
        internal static void PrintHierarchy(Transform transform, int outputNestCount = int.MaxValue)
        {
            PrintHierarchy(transform, "", outputNestCount);
        }

        [Conditional("DEBUG")]
        internal static void PrintHierarchy(string sceneName, int outputNestCount = int.MaxValue)
        {
            foreach (var obj in SceneManager.GetSceneByName(sceneName).GetRootGameObjects())
            {
                Logger.PrintHierarchy(obj.transform, "|", outputNestCount);
            }
        }

        [Conditional("DEBUG")]
        private static void PrintHierarchy(Transform transform, string spacing = "", int outputNestCount = int.MaxValue)
        {
            foreach (var child in transform.Cast<Transform>().ToList())
            {
                var text = child.GetComponent<TMPro.TextMeshProUGUI>();
                if (spacing.Length > outputNestCount) continue;
                if (text != null)
                {
                    Logger.Debug($"{spacing}{child.name} ( {text.text} )");
                }
                else
                {
                    Logger.Debug($"{spacing}{child.name}");
                }
                PrintHierarchy(child, spacing + "|");
            }
        }
    }
}
