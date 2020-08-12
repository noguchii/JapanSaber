using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace JapanSaber
{
    public static class Utils
    {
        public static async Task<Texture2D> GetTextureFromResourceAsync(string resourceName, CancellationToken cancellationToken)
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            using (var reader = new MemoryStream())
            {
                await stream.CopyToAsync(reader, 1024, cancellationToken);
                var texture = new Texture2D(2, 2);
                texture.LoadImage(reader.ToArray());

                return texture;
            }
        }

        public static async Task<Texture2D> GetTextureFromFileAsync(string filename, CancellationToken cancellationToken)
        {
            using (var stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var reader = new MemoryStream())
            {
                await stream.CopyToAsync(reader, 1024, cancellationToken);
                var texture = new Texture2D(2, 2);
                texture.LoadImage(reader.ToArray());

                return texture;
            }
        }

        public static IEnumerable<GameObject> FindChild(this GameObject self, string name)
        {
            var list = new List<Transform>();
            FindChildNest(self.transform, name, list);

            return list.Select(t => t.gameObject);
        }

        private static void FindChildNest(Transform parent, string name, List<Transform> result)
        {
            foreach (var child in parent.Cast<Transform>().ToList())
            {
                if (child.name == name)
                {
                    result.Add(child);
                }
                FindChildNest(child, name, result);
            }
        }
    }
}
