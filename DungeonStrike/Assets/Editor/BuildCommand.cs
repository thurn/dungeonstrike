using AssetBundles;
using UnityEditor;

namespace DungeonStrike
{
    public class BuildCommand
    {
        [MenuItem("Tools/Build OSX Player")]
        public static void BuildStandalonePlayer()
        {
            BuildScript.BuildStandalonePlayer();
        }
    }
}