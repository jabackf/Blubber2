using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Note: PALETTE must be in the global.dirPalettes directory.
//UI positioning is not automatic and is setup for palettes of 36 colors divided into rows of 12.

public class ColorPicker : MonoBehaviour
{
    public Texture2D texture;
    public int rowLength = 12;
    private GameObject color; //A reference to the prefab for a single color entry
    private GameObject paletteContainer;
    public bool pausePlayer = true; //If true, we will send a pauseCharacter() and unpauseCharacter message to the object with a Player tag in the Start and OnDestroy for this color picker.
    private GameObject player;
    private List<GameObject> recieveMessages = new List<GameObject>(); //This is a list of objects that should recieve OnColorHighlighted(Color) and OnColorSelected(Color) messages. List is empty by default. Use addMessageObject(GameObject) to add objects to this list.
    private int colorCount = 0;
    private Color selectedColor; //This will hold the currently highlighted color
    private Vector2 defaultColor = new Vector2(0, 1); //This is the position of the color in the palette that is selected by default at the start of the color picker
    
    private GameObject selectIcon;
    private Vector2 selectPos = new Vector2(0f, 0f);
    private Vector2 selectPosPrevious = new Vector2(0f, 0f);

    public List<GameObject> colors = new List<GameObject>();

    private Vector2 colorSize;
    public Vector2 colorSpacing = new Vector2(1.2f, 1.2f);
    public Vector2 palettePositionOffset = new Vector2(0f, -6f);
    public Vector2 selectIconOffset = new Vector2(0f, 0f);

    private Canvas canvas;

    private Sprite[] sprites;
    private Rect pcRect;
    private Global global;

    // Start is called before the first frame update
    void Awake()
    {
        global = GameObject.FindWithTag("global").GetComponent<Global>();
        sprites = Resources.LoadAll<Sprite>(global.dirPalettes+texture.name);
        canvas = GetComponent<Canvas>();

        color = FindInChildWithTag(gameObject, "menuItem");
        paletteContainer = FindInChildWithTag(gameObject, "menu");
        pcRect = paletteContainer.GetComponent<RectTransform>().rect;
        selectIcon = FindInChildWithTag(gameObject, "menuSelector");

        selectPos = defaultColor;

        player = GameObject.FindWithTag("Player");
        if (player && pausePlayer) player.SendMessage("pauseCharacter", SendMessageOptions.DontRequireReceiver);

        var rectTransform = color.GetComponent<RectTransform>();
        colorSize = new Vector2(rectTransform.rect.width, rectTransform.rect.height);

        int i = 0;
        for (int y = 0; y < sprites.Length / rowLength; y++)
        {
            for (int x = 0; x < rowLength; x++)
            {
                Vector3 pos = new Vector3(color.transform.position.x+ palettePositionOffset.x + (x * colorSize.x * colorSpacing.x), color.transform.position.y - pcRect.y -palettePositionOffset.y - (y * colorSize.y * colorSpacing.y), 0f);
                colors.Add(Instantiate(color));
                colors[colors.Count - 1].GetComponent<Image>().sprite = sprites[i];
                colors[colors.Count - 1].transform.parent = color.transform.parent;
                colors[colors.Count - 1].transform.position = pos;
                i += 1;
            }
        }

        colorCount = colors.Count;

        Destroy(color);
    }


    public bool highlightColor(Color c)
    {
        Color p;
        for (int i=0; i< colorCount; i++)
        {
            p = texture.GetPixel(((int)sprites[i].rect.width * i) + 4, 4);
            if (p==c) //Match found!
            {
                selectPos.y = Mathf.Floor(i / rowLength);
                selectPos.x = i-(selectPos.y*rowLength);
                return true;
            }
        }
        return false;
    }

    public void addMessageObject(GameObject o)
    {
        recieveMessages.Add(o);
    }

    // Update is called once per frame
    void Update()
    {
        selectPosPrevious = selectPos;
        if (Input.GetButtonDown("MenuUp") )
        {
            selectPos.y -= 1;
            if (selectPos.y < 0) selectPos.y = (sprites.Length / rowLength) - 1;
        }
        if (Input.GetButtonDown("MenuDown") )
        {
            selectPos.y += 1;
            if (selectPos.y > (sprites.Length / rowLength) - 1) selectPos.y = 0;
        }
        if (Input.GetButtonDown("MenuLeft") )
        {
            selectPos.x -= 1;
            if (selectPos.x <0) selectPos.x = rowLength-1;
        }
        if (Input.GetButtonDown("MenuRight") )
        {
            selectPos.x += 1;
            if (selectPos.x >= rowLength) selectPos.x = 0;
        }

        int i = (int)(selectPos.x + (selectPos.y * rowLength));

        selectIcon.transform.position = colors[i].transform.position + new Vector3(selectIconOffset.x, selectIconOffset.y, 0f);

        if (selectPosPrevious!=selectPos)
        {
            selectedColor = texture.GetPixel(( (int)sprites[i].rect.width * i) + 4, 4);

            foreach (var g in recieveMessages)
            {
                g.SendMessage("OnColorHighlighted", selectedColor, SendMessageOptions.DontRequireReceiver);
            }
        }

        if (Input.GetButtonDown("MenuSelect"))
        {
            foreach (var g in recieveMessages)
            {
                g.SendMessage("OnColorSelected", selectedColor, SendMessageOptions.DontRequireReceiver);
            }
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        if (player && pausePlayer) player.SendMessage("unpauseCharacter", SendMessageOptions.DontRequireReceiver);
    }

    public GameObject FindInChildWithTag(GameObject parent, string tag)
    {
        Transform t = parent.transform;
        foreach (Transform tr in t)
        {
            if (tr.tag == tag)
            {
                return tr.gameObject;
            }
            else
            {
                //Recursively look through children's children
                GameObject result = FindInChildWithTag(tr.gameObject, tag);
                if (result != null) return result;
            }
        }
        return null;
    }
}
