using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
 
 //This script fades in/out ui images. Used for scene fade transition.
 //This is a modified script based on one uploaded by Marco at the following URL 
 //https://gamedevelopertips.com/unity-how-fade-between-scenes/
 
public class fade : MonoBehaviour {
 
    public RawImage fadeOutUIImage;
    public float fadeSpeed = 0.8f; 
 
    public enum FadeDirection
    {
        In, //Alpha = 1
        Out // Alpha = 0
    }
	
	
	public bool notifyMapSystemOnComplete = true; //Since this object will primarily be used to fade in and out during scene transitions, checking this option will find the gloabl MapSystem object and call MapSystem::fadeComplete()
    private MapSystem msCaller; //Stores the map object if the above option is checked
	public bool destroyOnComplete = true; //If true, the object will self destruct when the fade is complete
	public UnityEvent OnFadeComplete; //Optional callback to trigger when fade is complete. You can specify this callback in the editor
	
	[Space]
    [Header("FadeIn= 0 to 1, FadeOut= 1 to 0")]
	public FadeDirection fadeDirection;
 
    #region MONOBHEAVIOR
    void Awake()
    {
		if (notifyMapSystemOnComplete)
			msCaller = GameObject.FindWithTag("global").GetComponent<Global>().map;
		
		//Start with a solid color if the image is fading from 1 to 0
		if (fadeDirection == FadeDirection.Out) {fadeOutUIImage.color = new Color (fadeOutUIImage.color.r,fadeOutUIImage.color.g, fadeOutUIImage.color.b, 1);}
        StartCoroutine(Fade());
		
    }
    #endregion
         
    #region FADE
    private IEnumerator Fade() 
    {
        float alpha = (fadeDirection == FadeDirection.Out)? 1 : 0;
        float fadeEndValue = (fadeDirection == FadeDirection.Out)? 0 : 1;
        if (fadeDirection == FadeDirection.Out) {
            while (alpha >= fadeEndValue)
            {
                SetColorImage (ref alpha, fadeDirection);
                yield return null;
            }
			if (msCaller!=null && notifyMapSystemOnComplete)
			{
				msCaller.fadeComplete();
			}
			if (OnFadeComplete!=null)
			{
				OnFadeComplete.Invoke();
			}
			if (destroyOnComplete) Destroy(gameObject);
        } else {
           
            while (alpha <= fadeEndValue)
            {
                SetColorImage (ref alpha, fadeDirection);
                yield return null;
            }
			if (msCaller!=null && notifyMapSystemOnComplete)
			{
				msCaller.fadeComplete();
			}
			if (OnFadeComplete!=null)
			{
				OnFadeComplete.Invoke();
			}
			if (destroyOnComplete) Destroy(gameObject);
        }
    }

    #endregion
 
    #region HELPERS

	public void setMapSystemCaller(MapSystem ms){
		this.msCaller = ms;
	}

    private void SetColorImage(ref float alpha, FadeDirection fadeDirection)
    {
        fadeOutUIImage.color = new Color (fadeOutUIImage.color.r,fadeOutUIImage.color.g, fadeOutUIImage.color.b, alpha);
        alpha += Time.deltaTime * (1.0f / fadeSpeed) * ((fadeDirection == FadeDirection.Out)? -1 : 1) ;
    }
    #endregion
}