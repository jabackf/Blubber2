using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogBox : MonoBehaviour
{
    public GameObject dBox;
    public Transform follow;
    public int lineBreakWidth = 350; //This max number of characters a string can have before a linebreak is used. The linebreak will replace the nearest space behind this number.

    private GameObject bg;
    private GameObject tail;
    private Text txtTitle;
    private Text txtMessage;


    public string title="Mr. Sign";
    public string text = "Hello! I'm Mr. Sign. I'm the best sign in the world.";// Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur.";


    // Start is called before the first frame update
    void Start()
    {
        bg = dBox.transform.Find("bg").gameObject;
        tail = dBox.transform.Find("tail").gameObject;
        txtTitle = bg.transform.Find("txtTitle").gameObject.GetComponent<Text>();
        txtMessage = bg.transform.Find("txtMessage").gameObject.GetComponent<Text>();

        txtTitle.text = title;
        txtMessage.text = breakLine(text, lineBreakWidth);

        //width = minWidth;
        //height = minHeight;

    }

    public string breakLine(string text, int width)
    {
        int l = text.Length;
        if (l < width) return text;
        else
        {
            string output = "";
            for (int i = 0; i < System.Math.Floor((float)(l / width))-1; i++)
            {
                string s = text.Substring(i * width, (i + 1) * width);
                int ind = s.LastIndexOf(' ');
                var aStringBuilder = new StringBuilder(s);
                aStringBuilder.Remove(ind, 1);
                aStringBuilder.Insert(ind, "\n");
                output += aStringBuilder.ToString();
            }
            return output;
        }
    }

    void OnValidate()
    {
        txtTitle.text = title;
        txtMessage.text = breakLine(text, lineBreakWidth);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 pos = Camera.main.WorldToScreenPoint(follow.position);
        dBox.transform.position = pos;
    }

    void OnGUI()
    {

    }
}
