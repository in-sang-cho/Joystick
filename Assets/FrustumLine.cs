using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrustumLine : MonoBehaviour
{
    private Camera mainCamera;

    private Vector3[] CameraFrustum = new Vector3[4];
    [SerializeField] private List<MeshRenderer> RendererList = new List<MeshRenderer>();

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

        for (int i = 0; i < CameraFrustum.Length; ++i)
        {
            var worldSpaceCorner = mainCamera.transform.TransformVector(CameraFrustum[i]);
            Debug.DrawLine(mainCamera.transform.localPosition, worldSpaceCorner, Color.black);
        }



        // ** Ray

        // **


    }

    void FindRenderer(GameObject _Obj)
    {
        for (int i = 0; i < _Obj.transform.childCount; ++i)
        {
            if (_Obj.transform.childCount > 0)
                FindRenderer(_Obj.transform.GetChild(i).gameObject);

            MeshRenderer renderer = _Obj.transform.GetChild(i).GetComponent<MeshRenderer>();

            if (renderer != null)
                RendererList.Add(renderer);
        }
    }

    IEnumerator Create()
    {
        while (true)
        {
            yield return new WaitForSeconds(3.0f);

            if (transform.childCount >= 1)
                continue;

            RendererList.Clear();

            // ** 게임 오브젝트를 변경
            //FindRenderer();

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
