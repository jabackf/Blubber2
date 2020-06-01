using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lineArrow : MonoBehaviour
{
    public float angle=45f;
    public float length;
    public float initialLength = 3f;
    public Color endColor = Color.red;
    public Color startColor = Color.yellow;
    public float startWidth = 0.15f;
    public float endWidth = 0.05f;
    public string spriteResource = "sprArrowHead";
    public string sortingLayerName = "InGameGUI";
    public Vector2 offset = new Vector2(0f, 0f);
    public bool restrainRotation = true; //If true, the angle will stop at max and min angles. If false, it will loop all the way around continuously.
    public float maxAngle = 180f;
    public float minAngle = 0f;
    public float maxLength = 4f;
    public float minLength = 1.4f;

    public bool isChild = true;    //Set this to true if the arrow is a child of the object it is "following"

    private GameObject arrowHeadGO;
    private LineRenderer lr; //Line renderer

    // Start is called before the first frame update
    void Awake()
    {
        lr = gameObject.AddComponent<LineRenderer>() as LineRenderer;
        gameObject.AddComponent<freezeZRotation>();
        lr.sortingLayerName = sortingLayerName;
        lr.material = Resources.Load("Materials/Flat", typeof(Material)) as Material;
        lr.useWorldSpace = false;
        lr.SetVertexCount(2);
        lr.startColor = startColor;
        lr.endColor = endColor;
        lr.startWidth = startWidth;
        lr.endWidth = endWidth;

        length = initialLength;

        arrowHeadGO = new GameObject();
        arrowHeadGO.name = gameObject.name+"_arrowHead";
        arrowHeadGO.transform.localPosition = new Vector3(0f, 0f, 0f);
        arrowHeadGO.transform.parent = gameObject.transform.parent;
        SpriteRenderer renderer = arrowHeadGO.AddComponent<SpriteRenderer>();
        renderer.sprite = Resources.Load(GameObject.FindWithTag("global").GetComponent<Global>().dirIcons+spriteResource, typeof(Sprite)) as Sprite;
        renderer.sortingLayerName = sortingLayerName;
        renderer.sortingOrder = 1;

        calculate();
    }

    public void show()
    {
        length = initialLength; //Reset the length
        lr.enabled = true;
        arrowHeadGO.SetActive(true);
        calculate();
    }

    public bool isShowing()
    {
        return lr.enabled;
    }

    public void hide()
    {
        lr.enabled = false;
        arrowHeadGO.SetActive(false);
    }

    public void follow(Transform t)
    {
        if (!isChild) gameObject.transform.position = t.position;
        
        if (lr.enabled)
        {
            calculate();
        }
    }

    public void setMinMax(float min, float max)
    {
        this.minAngle = min;
        this.maxAngle = max;
    }

    public void setAngle(float newAngle)
    {
        this.angle = newAngle;
        if (restrainRotation)
        {
            if (angle > maxAngle) angle = maxAngle;
            if (angle < minAngle) angle = minAngle;
        }
        else
        {
            if (angle > 360) angle -= 360;
            if (angle < 0) angle += 360;
        }
        if (lr.enabled)
        {
            calculate();
        }
    }

    public void setLength(float newLength)
    {
        this.length = newLength;
        if (this.length > maxLength) this.length = maxLength;
        if (this.length < minLength) this.length = minLength;
        if (lr.enabled)
        {
            calculate();
        }
    }

    public void calculate()
    {
        float radAngle = (-angle+90) * Mathf.Deg2Rad;
        lr.SetPosition(0, new Vector3(Mathf.Sin(radAngle),Mathf.Cos(radAngle),0f) + new Vector3(offset.x,offset.y,0));
        Vector3 endPos = new Vector3(Mathf.Sin(radAngle) * length ,Mathf.Cos(radAngle) * length, 0f) + new Vector3(offset.x, offset.y, 0);
        lr.SetPosition(1, endPos);
        arrowHeadGO.transform.position = new Vector3(gameObject.transform.position.x+endPos.x,gameObject.transform.position.y+endPos.y,0);
        arrowHeadGO.transform.rotation = Quaternion.Euler(new Vector3(0, 0,angle-90 ));
    }
}
