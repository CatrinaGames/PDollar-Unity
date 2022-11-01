using System.Collections;
using System.Collections.Generic;
using PDollarGestureRecognizer;
using TMPro;
using UnityEngine;

public class Demo2D : MonoBehaviour
{
    public Camera DrawCamera;
    public RectTransform DrawCanvas;

    public TMP_InputField xmlValue;
    public TMP_Text ResultText;
    public TMP_InputField gestureName;

    public PDollarRecognizer Recognizer;

    // Start is called before the first frame update
    void Start()
    {
        //Ajustar camara de dibujado
        DrawCamera.orthographicSize = DrawCanvas.position.y;
        DrawCamera.transform.position = new Vector3(DrawCanvas.position.x, DrawCanvas.position.y, -10);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SaveTest(){
        if(gestureName.text == ""){
            ResultText.text = $"PUT IT A NAME!";
            return;
        }
        Recognizer.SaveToTest(gestureName.text);
        Recognizer.ClearLine();
    }


    public void OnRecognized(PDollarResult result){
        ResultText.text = $"RESULT || Gesture: {result.GestureName} || Score: {result.score*100}";
        string xmlV = Recognizer.GetXML(gestureName.text);
        xmlValue.text = xmlV;
    }
}
