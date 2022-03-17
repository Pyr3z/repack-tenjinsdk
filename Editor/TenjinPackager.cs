using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using UnityEditor;

using UnityEngine;

namespace Tenjin
{
  //a simple wrapper class because Unity's json serializer can't deal with primitives..
  [Serializable]
  internal class ManifestWrapper
  {
    public string[] files;
  }

  public static class Exporter
  {
    static void Package()
    {
      var files = TenjinPackager.LoadManifest();
      TenjinPackager.PublishPackage(files, interactive: false);
    }

    [MenuItem("Tenjin/Export Package")]
    internal static void PackageInteractively()
    {
      var files = TenjinPackager.LoadManifest();
      TenjinPackager.PublishPackage(files, interactive: true);
    }
  }

  internal class TenjinPackager
  {
    //const string MANIFEST_PATH = "Assets/tenjin.unitypackage.manifest";
    const string MANIFEST_PATH = "Packages/com.tenjin.sdk/tenjin-repack.manifest";
    //public const string EXPORTED_PACKAGE_PATH = "Tenjin-repack.unitypackage";

    public static string ExportPath
    {
      get
      {
        var ass = System.Reflection.Assembly.GetExecutingAssembly();
        var pkg = UnityEditor.PackageManager.PackageInfo.FindForAssembly(ass);
        return $"Tenjin-{pkg.version}.unitypackage";
      }
    }

    internal static void SaveManifestFile(IEnumerable<String> assets)
    {
      if (File.Exists(MANIFEST_PATH))
        File.Delete(MANIFEST_PATH);

      var wtf = new ManifestWrapper() { files = assets.ToArray() };
      var json = JsonUtility.ToJson(wtf);

      var writer = new StreamWriter(MANIFEST_PATH, false);

      writer.WriteLine(json);
      writer.Close();
    }

    internal static IEnumerable<string> LoadManifest()
    {
      var reader = new StreamReader(MANIFEST_PATH);
      var jsonString = reader.ReadToEnd();
      reader.Close();

      var wrappedJson = JsonUtility.FromJson<ManifestWrapper>(jsonString);

      return wrappedJson.files;
    }

    internal static void PublishPackage(IEnumerable<string> enumerable, bool interactive)
    {
      PublishPackage(enumerable, ExportPath, interactive);
    }
    internal static void PublishPackage(IEnumerable<string> enumerable, string path, bool interactive)
    {
      if (File.Exists(path))
        File.Delete(path);

      var options = ExportPackageOptions.IncludeDependencies;
      if (interactive)
        options = options | ExportPackageOptions.Interactive;
      var filePaths = enumerable.ToArray();

      Debug.Log("Exporting files :\n" + string.Join("\n", filePaths));

      AssetDatabase.ExportPackage(filePaths, path, options);
    }


  }
}
