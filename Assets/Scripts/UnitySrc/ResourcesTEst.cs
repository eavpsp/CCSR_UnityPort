using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class ResourcesTEst : MonoBehaviour
{
    public TextMeshProUGUI text;
    // Start is called before the first frame update
    void Start()
    {
        GameObject myPrefab = Resources.Load<GameObject>("Game/MyPrefab");
        text.text = (myPrefab == null).ToString();
        IOManager_Switch.LoadAOC("test");


    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
