using UnityEngine;

public class UVScroll : MonoBehaviour
{
    [SerializeField] private string _uvName = "_MainTex";
    [SerializeField] private Vector2 _offset = Vector2.zero;
    [SerializeField] private Material _material = null;

    void Update()
    {
        var currentOffset = _material.GetTextureOffset(_uvName);
        _material.SetTextureOffset(_uvName, currentOffset + _offset * Time.deltaTime);
    }
}
