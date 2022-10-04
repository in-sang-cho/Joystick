using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadingBar : MonoBehaviour
{
    //[SerializeField]
    private Image Blue_Gradient = null;

    private float Crossline;

    private void Awake()
    {
        Blue_Gradient = GameObject.Find("Blue_Gradient").GetComponent<Image>();
    }

    IEnumerator Start()
    {
        // ** 
        Crossline = 0.7f;
        Blue_Gradient.fillAmount = 0;
        float Frame = 0.2f;

        while(true)
        {
            if (Crossline >= Blue_Gradient.fillAmount)
            {
                if (Blue_Gradient.fillAmount >= 0.85)
                    Frame = 4.5f;

                Blue_Gradient.fillAmount += Time.deltaTime;

                if (Blue_Gradient.fillAmount >= 1.0f)
                    break;
            }
            else
            {
                yield return new WaitForSeconds(Frame);
                Crossline += 0.1f;
            }

            yield return null;
        }

        Debug.Log("It's Time to Next Scene");

        //SceneManager.LoadScene("MainMenuScene");
    }
}
