using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]
public class Point : MonoBehaviour
{
    [HideInInspector] public Transform Node;

    private void Awake()
    {
        Rigidbody rigid = GetComponent<Rigidbody>();
        rigid.constraints = RigidbodyConstraints.FreezeRotation;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(this.transform.position, 1.0f);


        if (Node)
        {
            Gizmos.DrawLine(transform.position, Node.transform.position);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.transform.tag != "Ground")
        {
            transform.position = new Vector3(
                Random.Range(-25.0f, 25.0f),
                Random.Range(10.0f, 25.0f),
                Random.Range(-25.0f, 25.0f));

            transform.eulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
        }
        else
        {
            /*
            GameObject Obj = Instantiate(Resources.Load("Prefab/BoxEnemy") as GameObject);

            Obj.transform.position = transform.position;
            
            Obj.transform.parent = transform;

            MeshRenderer meshRenderer = Obj.GetComponent<MeshRenderer>();

            meshRenderer.material.shader = Shader.Find("Transparent/VertexLit");

            if (meshRenderer.material.HasProperty("_Color"))
            {
                Color color = meshRenderer.material.GetColor("_Color");

                meshRenderer.material.SetColor("_Color", new Color(color.r, color.g, color.b, 0.0f));

                StartCoroutine(SetColor(meshRenderer, color));
            }
             */
        }
    }

    /*
    IEnumerator SetColor(MeshRenderer meshRenderer, Color color)
    {

        float fTime = 0.0f;
        while (fTime <= 255.0f)
        {
            yield return null;

            fTime += Time.deltaTime;

            meshRenderer.material.SetColor("_Color", new Color(color.r, color.g, color.b, 0.0f));
        }
    }
     */
}
