using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//This is the script that operates the gameplay ui overlay. So this handles things like showing/hiding the currently-holding text.

public class UIOverlayScript : MonoBehaviour
{
    public Text holdingText;
    public RectTransform holdingTextPanelRect;

    //Used for hiding / showing the holding object panel
    float holdingMoveSpeed = 20f;
    private bool holdingHide = true;
    private float holdingYOffset = 0f, holdingYDestination = 32f;
    private float holdingYInitial;
    private float holdingYVelocity = 0;

    // Start is called before the first frame update
    void Start()
    {
        holdingYInitial = holdingTextPanelRect.position.y;
        if (holdingText.text == "")
        {
            holdingHide = true;
            holdingYOffset = holdingYDestination;
        }
        else holdingHide = false;
    }

    void Update()
    {
        holdingYOffset = Mathf.SmoothDamp(holdingYOffset, (holdingHide ? holdingYDestination : 0f), ref holdingYVelocity, holdingMoveSpeed * Time.deltaTime);
        holdingTextPanelRect.position = new Vector3(holdingTextPanelRect.position.x, holdingYInitial-holdingYOffset, 0f);

    }

    public void setHoldingText(string text)
    {
        CancelInvoke("clearHoldingText");
        if (text == "")
        {
            holdingHide = true;
            Invoke("clearHoldingText", 0.7f);
        }
        else
        {
            holdingText.text = text;
            holdingHide = false;
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(holdingTextPanelRect);
    }
    private void clearHoldingText()
    {
        holdingText.text = "";
    }
}
