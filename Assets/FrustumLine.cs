using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrustumLine : MonoBehaviour
{
    private Camera mainCamera;

    [SerializeField] private Vector3[] CameraFrustum = new Vector3[4];
    [SerializeField] private List<MeshRenderer> RendererList = new List<MeshRenderer>();
    [SerializeField] private List<GameObject> CullingList = new List<GameObject>();

    [SerializeField] private LayerMask mask;
    [SerializeField] private float Distance;

    //private 

    [Range(0.0f, 1.0f)]
    private float X, Y, CX, CY;

    // Start is called before the first frame update
    private void Awake()
    {
        X = 0.45f;
        Y = 0.45f;
        CX = 0.1f;
        CY = 0.1f;

        Distance = 13.0f;

        for (int i = 0; i < CameraFrustum.Length; ++i)
            CameraFrustum[i] = new Vector3(0.0f, 0.0f, 0.0f);

        mainCamera = transform.GetComponent<Camera>();
    }

    private void FixedUpdate()
    {
        mainCamera.CalculateFrustumCorners(
            new Rect(X, Y, CX, CY),
            mainCamera.farClipPlane,
            Camera.MonoOrStereoscopicEye.Mono,
            CameraFrustum);

        /*
        for (int i = 0; i < CameraFrustum.Length; ++i)
            CameraFrustum[i].Normalize();
         */

        CullingList.Clear();

        for (int i = 0; i < CameraFrustum.Length; ++i)
        {
            var worldSpaceCorner = mainCamera.transform.TransformVector(CameraFrustum[i]);
            Debug.DrawLine(mainCamera.transform.localPosition, worldSpaceCorner, Color.black);

            Ray ray = new Ray(mainCamera.transform.position, worldSpaceCorner);

            RaycastHit[] hits = Physics.RaycastAll(ray, Distance, mask);

            foreach (RaycastHit hit in hits)
                if (!CullingList.Contains(hit.transform.gameObject))
                    CullingList.Add(hit.transform.gameObject);
        }

        RendererList.Clear();

        foreach (GameObject Element in CullingList)
            StartCoroutine(FindRenderer(Element));

        foreach (MeshRenderer Element in RendererList)
        {
            Element.material.shader = Shader.Find("Transparent/VertexLit");

            if (Element.material.HasProperty("_Color"))
            {
                Color color = Element.material.GetColor("_Color");

                StartCoroutine(SetColor(Element, color));
            }
        }
    }

    IEnumerator FindRenderer(GameObject _Obj)
    {
        int i = 0;

         do
         {
             if (_Obj.transform.childCount > 0)
                 FindRenderer(_Obj.transform.GetChild(i).gameObject);
         
             // ** 현재 것부터 확인하기 위해(만약에 Child 것을 추가하고 싶으면 transform.GetChild(i) 로 변경할 것)
             MeshRenderer renderer = _Obj.transform.GetChild(i).GetComponent<MeshRenderer>();
         
             if (renderer != null)
                 RendererList.Add(renderer);
         }
         while (i < _Obj.transform.childCount);

        yield return null;
    }


    IEnumerator Create()
    {
        while (true)
        {
            yield return new WaitForSeconds(3.0f);

            if (transform.childCount >= 1)
                continue;

            RendererList.Clear();

            foreach (MeshRenderer meshRenderer in RendererList)
            {
                meshRenderer.material.shader = Shader.Find("Transparent/VertexLit");

                if (meshRenderer.material.HasProperty("_Color"))
                {
                    Color color = meshRenderer.material.GetColor("_Color");

                    meshRenderer.material.SetColor("_Color", new Color(color.r, color.g, color.b, 0.0f));

                    StartCoroutine(SetColor(meshRenderer, color));
                }
            }
        }
    }

    IEnumerator SetColor(MeshRenderer meshRenderer, Color color)
    {
        float fTime = 1.0f;

        while (fTime > 0.5f)
        {
            yield return null;

            fTime -= Time.deltaTime + 1.5f;

            meshRenderer.material.SetColor("_Color", new Color(color.r, color.g, color.b, fTime));
        }
    }

}
