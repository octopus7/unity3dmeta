using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(metasearch))]
public class metasearchtEditor : Editor
{
    Dictionary<string, HashSet<string>> dicRefGuid = new Dictionary<string, HashSet<string>>();

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        metasearch myTarget = (metasearch)target;

        if (GUILayout.Button("FindAssets"))
        {
            string [] guids = AssetDatabase.FindAssets(myTarget.keyword);
            myTarget.log = "Find : " + myTarget.keyword;
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                myTarget.log += "\n" + guid + " : " + path;
            }
        }

        if (GUILayout.Button("Scan All assets"))
        {
            // Retrieves only the assets of the project
            List<string> paths = new List<string>(AssetDatabase.GetAllAssetPaths());
            List<string> assetsPaths = paths.Where(item => item.Contains("Assets/")).ToList();

            string log = "";
            dicRefGuid.Clear();

            int countAssets = 0;
            int countREFs = 0;

            foreach (string path in assetsPaths)
            {
                countAssets++;

                string ownguid = AssetDatabase.AssetPathToGUID(path);
                if(countAssets<10) log += "\n" + ownguid + " : " + path ;                

                dicRefGuid[ownguid] = new HashSet<string>();
                // Do not recursive search because you need depth.
                string[] refPaths = AssetDatabase.GetDependencies(path, false); 

                foreach (string refpath in refPaths)
                {
                    countREFs++;
                    if (refpath == path) continue;
                    string refguid = AssetDatabase.AssetPathToGUID(refpath);
                    if (countAssets < 10) log += "\n\t - " + refguid + " : " + refpath;
                    dicRefGuid[ownguid].Add(refguid);
                }
            }

            myTarget.log = countAssets + " assets " + countREFs + " REFs\n" + log;
        }

        if (GUILayout.Button("Find GUID Reverse REF"))
        {
            myTarget.log = FindRef(myTarget.guid);
        }

        if (GUILayout.Button("Find Orphaned"))
        {
            string log = "Orphaned";

            HashSet<string> allhash = new HashSet<string>();
            foreach (var pair in dicRefGuid)
            {
                var hashset = pair.Value;
                allhash.UnionWith(hashset);
            }
            foreach (var pair2 in dicRefGuid)
            {
                string guid = pair2.Key;
                if ( pair2.Value.Count == 0 && !allhash.Contains(guid))
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    if (path.Contains("/Editor/")) continue;
                    if (AssetDatabase.IsValidFolder(path)) continue;

                    log += "\n" + guid + " : " + path;
                }
            }
            myTarget.log = log;
        }
    }

    private string FindRef(string guid, int maxDepth = 3)
    {
        string header = guid;

        string ownlog = "["+ guid + "] " + AssetDatabase.GUIDToAssetPath(guid);

        HashSet<string> foundGUID = new HashSet<string>();
        HashSet<string> searchGUID = new HashSet<string>();
        HashSet<string> nextGUID = new HashSet<string>();

        searchGUID.Add(guid);

        for (int depth = 1; depth <= maxDepth; depth++)
        {
            ownlog += " \r\ndepth :::: " + depth ;

            foreach (var pair in dicRefGuid)
            {
                string own = pair.Key; // owner
                var hashset = pair.Value; // reference

                foreach (string currentGUID in searchGUID)
                {
                    if (hashset.Contains(currentGUID))
                    {
                        if (!foundGUID.Contains(own) && !searchGUID.Contains(own) && !nextGUID.Contains(own))
                        {
                            ownlog += " \n";
                            string path = AssetDatabase.GUIDToAssetPath(own);
                            string currline = depth + "," + own + "," + path + " " + currentGUID;
                            ownlog += currline;
                            nextGUID.Add(own);
                        }
                    }
                }
            }

            foreach (var src in searchGUID)
            {
                foundGUID.Add(src);
            }
            searchGUID.Clear();
            foreach (var src in nextGUID)
            {
                searchGUID.Add(src);
            }
        }
        return ownlog;
    }
}