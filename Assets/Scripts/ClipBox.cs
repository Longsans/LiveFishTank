using UnityEngine;

[ExecuteAlways]
public class ClipBox : MonoBehaviour
{
    public string clipName = "_WorldToBox";

    void Update()
    {
        Shader.SetGlobalMatrix(clipName, transform.worldToLocalMatrix);
    }
}