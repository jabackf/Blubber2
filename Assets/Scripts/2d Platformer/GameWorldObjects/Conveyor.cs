using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Conveyor : MonoBehaviour
{
    public float speed = -100f;
    public bool printDebugInfo = false;

    public class objEntry
    {
        public GameObject gameObject;
        public Rigidbody2D rb;
        public CharacterController2D characterController;

        public objEntry(GameObject go)
        {
            this.gameObject = go;
            this.rb = this.gameObject.GetComponent<Rigidbody2D>();
            this.characterController = this.gameObject.GetComponent<CharacterController2D>();
            if (this.characterController != null) this.characterController.setIsOnConveyor(true);
        }
        ~objEntry()
        {
            if (this.characterController != null) this.characterController.setIsOnConveyor(false);
        }
    }

    List<objEntry> objects = new List<objEntry>();

    void FixedUpdate()
    {
        foreach(objEntry obj in objects)
        {
            //obj.rb.AddForce(new Vector2(speed, 0));
            if (printDebugInfo) Debug.Log(obj.gameObject.name + " < Applied Motion: POS:" + (new Vector2(obj.rb.position.x, obj.rb.position.y)) + "Spd: "+ new Vector2(speed, 0) + "Vel: "+ obj.rb.velocity);
            //if (obj.rb.velocity.y<3)
            obj.rb.MovePosition( new Vector2(obj.rb.position.x+speed* Time.fixedDeltaTime, obj.rb.position.y) + (obj.rb.velocity*Time.fixedDeltaTime) );
            //obj.rb.velocity = new Vector2( (obj.rb.velocity.x+speed) * Time.fixedDeltaTime, obj.rb.velocity.y);

            /*if (obj.characterController != null)
                obj.characterController.Move(speed * Time.fixedDeltaTime, false, false);
            else
                obj.rb.MovePosition(new Vector2(obj.gameObject.transform.position.x + speed * Time.fixedDeltaTime, obj.gameObject.transform.position.y));*/

        }
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        Rigidbody2D rb = other.gameObject.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            if (other.gameObject.transform.position.y > gameObject.transform.position.y)
            {
                if (printDebugInfo) Debug.Log(other.gameObject.name + " added");
                objects.Add(new objEntry(other.gameObject));
            }
        }
    }
    void OnCollisionExit2D(Collision2D other)
    {
        if (printDebugInfo) Debug.Log(other.gameObject.name + " collision exit");
        objects.RemoveAll(o => o.gameObject == other.gameObject);
    }
}
