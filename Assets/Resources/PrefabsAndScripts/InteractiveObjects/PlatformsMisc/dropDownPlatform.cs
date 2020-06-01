using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Ground layer objects with this script can be fallen through by calling DropObject(GameObject go)
//Once dropped, the object will stop colliding until the object exits the collision.
//For, for example, if the player might press "down" while standing on a platform to drop through it.

//NOTE: Drop platforms need two colliders! One for standing/walking on, and a slightly bigger one setup as a trigger. 
//The trigger serves to tell the script when the player has finished dropping through.

public class dropDownPlatform : MonoBehaviour
{
    List<GameObject> dropList = new List<GameObject>();

    public void DropObject(GameObject go)
    {
        if (dropList.Find(g => g == go) == null)
        {
            dropList.Add(go);
            Collider2D[] me = gameObject.GetComponents<Collider2D>();
            Collider2D[] you = go.GetComponents<Collider2D>();
            foreach (Collider2D m in me)
            {
                if (!m.isTrigger)
                {
                    foreach (Collider2D y in you)
                    {
                        Physics2D.IgnoreCollision(m, y, true);
                    }
                }
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (dropList.Find(g => g == other.gameObject) != null)
        {
            dropList.RemoveAll(g => g == other.gameObject);

            Collider2D[] me = gameObject.GetComponents<Collider2D>();
            Collider2D[] you = other.gameObject.GetComponents<Collider2D>();
            foreach (Collider2D m in me)
            {
                foreach (Collider2D y in you)
                {
                    Physics2D.IgnoreCollision(m, y, false);
                }
            }
        }
    }
}
