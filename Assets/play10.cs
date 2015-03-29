using UnityEngine;

public class buttonClick : MonoBehaviour
{
	public int buttonState;
    private int counter = 0;
    
    void OnClick ()
    {
        counter++;
        print ("Clicked " + counter.ToString () + " times.");
		buttonState = 1;
    }
}