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
            myTarget.lines = AssetDatabase.FindAssets(myTarget.keyword);
            myTarget.log = "Find : " + myTarget.keyword;
            foreach (string line in myTarget.lines)
            {
                string path = AssetDatabase.GUIDToAssetPath(line);
                myTarget.log += "\n" + line + " : " + path;
            }
        }

        if (GUILayout.Button("Scan All assets"))
        {
            // Retrieves only the assets of the project
            List<string> paths = new List<string>(AssetDatabase.GetAllAssetPaths());
            myTarget.lines = paths.Where(item => item.Contains("Assets/")).ToArray<string>();

            string log = "";
            dicRefGuid.Clear();

            foreach (string path in myTarget.lines)
            {
                string ownguid = AssetDatabase.AssetPathToGUID(path);
                log += "\n" + ownguid + " : " + path ;

                dicRefGuid[ownguid] = new HashSet<string>();
                // Do not recursive search because you need depth.
                string[] refGUIDs = AssetDatabase.GetDependencies(path, false); 

                foreach (string refpath in refGUIDs)
                {
                    if(refpath == path) continue;
                    string refguid = AssetDatabase.AssetPathToGUID(refpath);
                    log += "\n\t - " + refguid + " : " + refpath;
                    dicRefGuid[ownguid].Add(refguid);
                }
            }
            myTarget.log = log;
        }

        if (GUILayout.Button("Find GUID Reverse REF"))
        {
            myTarget.log = FindRef(myTarget.guid);
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
                            ownlog += " \r\n";
                            string path = AssetDatabase.GUIDToAssetPath(own);
                            string currline = "[" + header + "]," + depth + "," + own + "," + path + " " + currentGUID + "\r\n";
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