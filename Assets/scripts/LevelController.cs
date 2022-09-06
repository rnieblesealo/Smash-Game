using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelController : MonoBehaviour
{
    public static List<Collider> levelObjects = new List<Collider>();

    [SerializeField] private bool autoSearch = true;
    [SerializeField] private int groundLayer = 3;

    private void Awake()
    {
        if (autoSearch)
        {
            Collider[] colliders = FindObjectsOfType<Collider>();
            for (int i = 0; i < colliders.Length; ++i)
                if (colliders[i].gameObject.layer == groundLayer)
                    levelObjects.Add(colliders[i]);
        }
    }
}
