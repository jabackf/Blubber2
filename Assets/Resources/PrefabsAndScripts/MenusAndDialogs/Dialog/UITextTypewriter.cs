using UnityEngine;
using System.Collections;
using UnityEngine.UI;

// attach to UI Text component (with the full text already there)

public class UITextTypewriter : MonoBehaviour 
{

	Text txt;
    string story;
    public float waitTime = 0.005f;
    public bool done=false;
    public LayoutElement hSpacer; //If not null, this object will be used to maintain the width of the box before the text is added
    private Color color;
    private bool started = false;

    void Start()
    {
        txt = GetComponent<Text>();
        story = txt.text;
        color = txt.color;
        txt.color = new Color(0, 0, 0, 0);

    }

    void OnGUI()
    {
        if (!started)
        {
            started = true;

            if (hSpacer != null)
            {
                hSpacer.preferredWidth = gameObject.GetComponent<RectTransform>().rect.width;
            }

            txt.color = color;
            txt.text = "";

            StartCoroutine("PlayText");
        }
    }

    IEnumerator PlayText()
    {
        foreach (char c in story)
        {
            txt.text += c;
            yield return new WaitForSeconds(waitTime);
        }
        done = true;
    }

}