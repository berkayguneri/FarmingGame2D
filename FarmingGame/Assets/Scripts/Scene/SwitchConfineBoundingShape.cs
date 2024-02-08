using UnityEngine;
using Cinemachine;
public class SwitchConfineBoundingShape : MonoBehaviour
{

    private void OnDisable()
    {
        EventHandler.AfterSceneLoadEvent -= SwitchBoundingShape;
    }
    private void OnEnable()
    {
        EventHandler.AfterSceneLoadEvent += SwitchBoundingShape;
    }

    private void SwitchBoundingShape()
    {
        PolygonCollider2D polyCol2d = GameObject.FindGameObjectWithTag(Tags.BoundsConfiner).GetComponent<PolygonCollider2D>();

        CinemachineConfiner2D cineConf2D = GetComponent<CinemachineConfiner2D>();

        cineConf2D.m_BoundingShape2D = polyCol2d;

        cineConf2D.InvalidateCache();
    }
}
