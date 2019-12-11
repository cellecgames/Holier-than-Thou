﻿using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraController : MonoBehaviour
{
    GameObject myPlayer;
    CinemachineFreeLook myCamera;
    private List<RaycastHit> m_objectsInBetweenPlayerAndCamera;

    // Start is called before the first frame update
    void Start()
    {
        myPlayer = GetComponentInParent<Competitor>().gameObject;
        myCamera = GetComponent<CinemachineFreeLook>();
        m_objectsInBetweenPlayerAndCamera = new List<RaycastHit>();
    }

    // Update is called once per frame
    void Update()
    {
        SetAllTheseToTransparent(Physics.RaycastAll(transform.position, (myPlayer.transform.position - transform.position), (Vector3.Distance(transform.position, (myPlayer.transform.position)) ) ));

        RaycastHit hitSphere;
        if(Physics.SphereCast(myPlayer.transform.position, 1f,Vector3.up, out hitSphere,10f) || Physics.SphereCast(myCamera.transform.position, 1f,Vector3.up, out hitSphere, 10f) || Physics.SphereCast(new Vector3(myCamera.transform.position.x, myPlayer.transform.position.y, myCamera.transform.position.z), 1f,Vector3.up, out hitSphere, 10f))
        {
            myCamera.m_YAxis.Value = Mathf.Lerp(myCamera.m_YAxis.Value, .5f, .1f);
        }
        else
        {
            myCamera.m_YAxis.Value = Mathf.Lerp(myCamera.m_YAxis.Value, 1, .1f); ;
        }
    }

    private void SetAllTheseToTransparent(RaycastHit[] _hitsToSet) {
        List<RaycastHit> allHitsToSet = new List<RaycastHit>();

        foreach(RaycastHit hit in _hitsToSet) {
            if(hit.transform == myPlayer.transform || hit.transform == this.transform) {
                continue;
            }

            allHitsToSet.Add(hit);
        }

        foreach(RaycastHit hit in m_objectsInBetweenPlayerAndCamera) {
            SetAlphaToColorOnHit(myPlayer.GetComponent<Competitor>(), hit, 1.00f);
        }

        m_objectsInBetweenPlayerAndCamera.Clear();

        foreach(RaycastHit hit in allHitsToSet) {
            SetAlphaToColorOnHit(myPlayer.GetComponent<Competitor>(), hit, 0.25f);
            m_objectsInBetweenPlayerAndCamera.Add(hit);

        }
    }

    private void SetAlphaToColorOnHit(Competitor competitor, RaycastHit _raycastHit, float _alphaToSet) {

        if(competitor.isTerrain == true)
        {
            return;
        }
        else
        {
            if (_raycastHit.transform.GetComponent<MeshRenderer>())
            {
                Color originalColor = _raycastHit.transform.GetComponent<Renderer>().material.color;
                if(originalColor == null)
                {
                    return;
                }
                _raycastHit.transform.GetComponent<MeshRenderer>().material.DisableKeyword("_ALPHATEST_ON");
                _raycastHit.transform.GetComponent<MeshRenderer>().material.DisableKeyword("_ALPHABLEND_ON");
                _raycastHit.transform.GetComponent<MeshRenderer>().material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                _raycastHit.transform.GetComponent<MeshRenderer>().material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                _raycastHit.transform.GetComponent<MeshRenderer>().material.SetInt("_Zwrite", 0);
                _raycastHit.transform.GetComponent<MeshRenderer>().material.renderQueue = 3000;
                _raycastHit.transform.GetComponent<MeshRenderer>().material.color = new Color(originalColor.r, originalColor.g, originalColor.b, _alphaToSet);
                _raycastHit.transform.GetComponent<MeshRenderer>().material.SetFloat("_Mode", 3);

            }
        }
    }
    
}
