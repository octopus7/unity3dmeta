using System.Collections.Generic;
using UnityEngine;

public class metasearch : MonoBehaviour {

    public string keyword;
    public string [] lines;

    public string guid;

    Dictionary<string, HashSet<string>> dicRefGuid = new Dictionary<string, HashSet<string>>();

    [TextArea(3,20)]
    public string log;

    void Start () {
		
	}

    void Update () {
		
	}

}
