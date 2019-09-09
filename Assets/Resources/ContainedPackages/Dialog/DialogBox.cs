﻿using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogBox : MonoBehaviour
{
    public string title = "Mr. Sign";
    public string text = "Hello! I'm Mr. Sign. I'm the best sign in the world.";// Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur.";

    public GameObject dBox;
    public Transform followTop;  //The top point of the character/sign/whatever that the dbox should point to. Leave this variable and the bottom variable null to show the dialog in the center.
    public Transform followBottom; //The bottom part (used mainly if the character/sign/whatever is near the top of the screen, or used exclusively if no topFollow is provided)
    public int lineBreakWidth = 60; //This max number of characters a string can have before a linebreak is used. The linebreak will replace the nearest space behind this number.
    public Vector2 dbOffset = new Vector2(0f, .5f);
    public bool stayOnScreen = true; //If set to true, the script will adjust the dialog box to try to keep it on screen.

    private Image image;  //An image to display in the text box
    public string imgResource=""; //A resource location for an image to display in the box. Leave blank for none. Sprites/UnsortedSprites/RippedFromGMXPrototype/sprBirdWithArms_0
    public Sprite imgSprite = null;
    private RectTransform imgRect;

    public float maxWidth = 400; //Max width/height of dialog bg rect
    public float maxHeight = 300;

    private GameObject bg;
    private GameObject tail;
    private Text txtTitle;
    private Text txtMessage;
    private RectTransform bgRect;
    private RectTransform tailRect;
    private const float tailBufferX = 20; //How close the tail is allowed to get to the edges of the screen.

    [Space]
    [Header("Menu")]

    public GameObject selListPrefab;
    public GameObject selItemPrefab;
    public GameObject selectCursorPrefab;
    public int selectItemHeight = 20;
    private GameObject menuGo;
    private RectTransform menuRect;
    private GameObject selectCursor;
    public int selectedIndex = 0;
    public Vector2 cursorOffset = new Vector2(8, -8);
    [SerializeField] public List<string> answers = new List<string>(); //If list has two or more elements, a menu will be created
    
    

    // Start is called before the first frame update
    void Start()
    {
        bg = dBox.transform.Find("bg").gameObject;
        bgRect = bg.GetComponent<RectTransform>() as RectTransform;
        tail = dBox.transform.Find("tail").gameObject;
        tailRect = tail.GetComponent<RectTransform>() as RectTransform;
        txtTitle = bg.transform.Find("txtTitle").gameObject.GetComponent<Text>();
        txtMessage = bg.transform.Find("txtMessage").gameObject.GetComponent<Text>();

        image = bg.transform.Find("img").gameObject.GetComponent<Image>() as Image;
        imgRect = image.gameObject.GetComponent<RectTransform>() as RectTransform;
        LoadImage(imgResource);

        txtTitle.text = title;
        txtMessage.text = breakLine(text, lineBreakWidth);

        if (answers.Count > 1)
        {
            GenerateMenu();
        }

    }

    public void GenerateMenu()
    {
        if (menuGo != null) Destroy(menuGo);
        menuGo = Instantiate(selListPrefab);
        menuGo.transform.parent = bg.transform;
        menuRect = menuGo.GetComponent<RectTransform>() as RectTransform;
        foreach (string s in answers)
        {
            GameObject item = Instantiate(selItemPrefab);
            item.GetComponent<Text>().text = s;
            item.transform.parent = menuGo.transform;
            //answersGo.Add(item);
        }
        selectedIndex = 0;

        selectCursor = Instantiate(selectCursorPrefab);
        selectCursor.transform.parent = dBox.transform;
    }

    public void LoadImage(string resource)
    {
        imgResource = resource;
        if (resource != "")
        {
            imgSprite = Resources.Load<Sprite>(imgResource) as Sprite;
            if (imgSprite != null) image.sprite = imgSprite;
        }
        else
        {
            imgSprite = null;
            image.sprite = null;
        }
    }

    public string breakLine(string text, int width)
    {
        int l = text.Length;
        if (l <= width) return text;
        else
        {
            string output = "";
            while (l > width)
            {
                string s = text.Substring(0, width);
                text = text.Remove(0, width);
                l -= width;
                int ind = s.LastIndexOf(' ');
                if (ind == -1) ind = s.Length - 1;
                var aStringBuilder = new StringBuilder(s);
                aStringBuilder.Remove(ind, 1);
                aStringBuilder.Insert(ind, "\n");
                output += aStringBuilder.ToString();
            }

            return output+text;
        }
    }

    void OnValidate()
    {
        LoadImage(imgResource);
        txtTitle.text = title;
        txtMessage.text = breakLine(text, lineBreakWidth);
    }

    // Update is called once per frame
    void Update()
    {
        if (menuGo != null)
        {
            if (Input.GetButtonDown("MenuUp"))
            {
                selectedIndex -= 1;
                if (selectedIndex < 0) selectedIndex = answers.Count - 1;
            }
            if (Input.GetButtonDown("MenuDown"))
            {
                selectedIndex += 1;
                if (selectedIndex > answers.Count - 1) selectedIndex = 0;
            }
            if (Input.GetButtonDown("MenuSelect"))
            {
                Debug.Log("You selected: " + answers[selectedIndex]);
            }
        }
    }

    //Updates the positioning of the dialog box on the screen
    void UpdatePosition()
    {

        if (bgRect.rect.width>maxWidth || bgRect.rect.height > maxHeight)
        {

            Debug.Log("!");
            bgRect.sizeDelta = new Vector2(100, 100);
        }

        float halfW = bgRect.rect.width / 2;
        float halfH = bgRect.rect.height / 2;
        float tailHalfW = tailRect.rect.width / 2;
        float tailHalfH = tailRect.rect.height / 2;
        float tailX = 0;
        float tailY = 0;
        bool tailFlip = false;

        Vector3 pos = new Vector3(0f, 0f, 0f);

        if (followTop != null)
        {
            pos = Camera.main.WorldToScreenPoint(followTop.position + new Vector3(dbOffset.x, dbOffset.y, 0) );
            pos.y += halfH;
            tailY = pos.y - halfH - (tailHalfH);
        }

        //Move the box under the character if it would fit better on the screen, or if we've only specified a bottom transform
        if (followBottom != null && (pos.y > (Screen.height - halfH) || followTop == null))
        {
            pos = Camera.main.WorldToScreenPoint(new Vector3(followBottom.position.x + dbOffset.x, followBottom.position.y - dbOffset.y, 0));
            pos.y -= halfH;

            tailY = pos.y + halfH + (tailHalfH);
            tailFlip = true;
        }

        tailX = pos.x;
        if (tailX < tailBufferX) tailX = tailBufferX;
        if (tailX > (Screen.width - tailBufferX)) tailX = Screen.width - tailBufferX;

        if (stayOnScreen)
        {
            if (pos.x < halfW) pos.x = halfW;
            if (pos.x > (Screen.width - halfW)) pos.x = (Screen.width - halfW);
        }

        //If no transform is provided, just put the box in the middle of the screen
        if (followTop == null && followBottom == null)
        {
            pos = new Vector3((Screen.width / 2), (Screen.height / 2), 0f);
            tail.SetActive(false);
        }
        else
        {
            tail.SetActive(true);
            tail.transform.localScale = new Vector3(tail.transform.localScale.x, tailFlip ? -1 : 1, tail.transform.localScale.z);
            tail.transform.position = new Vector3(tailX, tailY, 0f);
        }


        dBox.transform.position = pos;

        //Resize the image
       /* if (imgSprite != null)
        {
            //imgRect.rect.width = 100;
            //Debug.Log(imgRect.rect.width);
            //if (imgRect.rect.width > imgMaxWidth) image.transform.localScale = new Vector3((1 / imgRect.rect.width) * imgMaxWidth, (1 / imgRect.rect.height) * imgMaxWidth);//imgRect.sizeDelta = new Vector2(imgMaxWidth, imgRect.rect.height); 
            if (imgRect.rect.height > imgMaxHeight) imgRect.sizeDelta = new Vector2(64,64);
        }*/
    }

    void UpdateMenu()
    {
        if (menuGo != null)
        {

            float halfW = menuRect.rect.width / 2;
            float halfH = menuRect.rect.height / 2;
            selectCursor.transform.position = new Vector3(menuGo.transform.position.x-halfW+cursorOffset.x, menuGo.transform.position.y+cursorOffset.y + halfH - (selectedIndex * selectItemHeight));
        }
    }

    void OnGUI()
    {
        UpdatePosition();
        UpdateMenu();
    }
}
