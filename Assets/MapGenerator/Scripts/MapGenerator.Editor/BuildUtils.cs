using UnityEngine;
using LaForge.MapGenerator.SimpleJSON;
using UnityEditor.PackageManager.Requests;
using UnityEditor.PackageManager;

public class BuildUtils
{
    static string package_path = "Assets/MapGenerator/MapGenerator.Shared";

    private static string MakePackageManifest()
    {
        JSONNode json = new JSONObject();
        json.Add("name", "laforge.mapgenerator.shared");
        json.Add("version", Application.version);
        json.Add("displayName", "MapGenerator.Shared");
        json.Add("description", "Assets and scripts needed to load maps generated by the La Forge's Map Generator.");
        json.Add("unity", "2020.2");
        json.Add("unityRelease", "4f1");

        JSONObject dependencies = new JSONObject();
        dependencies.Add("com.unity.ai.navigation.components", "1.0.0-exp.1");
        json.Add("dependencies", dependencies);

        JSONArray keywords = new JSONArray();
        keywords.Add("Reinforcement Learning");
        keywords.Add("Environment");
        keywords.Add("Procedural");
        json.Add("keywords", keywords);

        JSONObject author = new JSONObject();
        author.Add("name", "Ubisoft La Forge");
        author.Add("email", "philippe.marcotte@ubisoft.com");
        author.Add("url", "https://montreal.ubisoft.com/en/our-engagements/research-and-development/");
        json.Add("author", author);

        return json.ToString(4);
    }

    public static void MakePackage()
    {
        Mono.Options.OptionSet option = new Mono.Options.OptionSet();
        string directory = "./";
        option.Add("output-directory=", "Output directory", (string output_directory) => directory = output_directory);
        option.Parse(System.Environment.GetCommandLineArgs());

        string package_json_path = System.IO.Path.Combine(package_path, "package.json");
        System.IO.File.WriteAllText(package_json_path, MakePackageManifest());
        PackRequest request = Client.Pack(package_path, System.IO.Path.GetFullPath(directory));
        while (!request.IsCompleted)
            System.Threading.Thread.Sleep(500);
        if (request.Status == StatusCode.Failure)
            throw new System.Exception(request.Error.message);
    }

    public static void SetVariant()
    {
        Mono.Options.OptionSet option = new Mono.Options.OptionSet();
        option.Add("variant", "Application variant", (string v) => UnityEditor.PlayerSettings.bundleVersion = v);
        option.Parse(System.Environment.GetCommandLineArgs());
    }
}
