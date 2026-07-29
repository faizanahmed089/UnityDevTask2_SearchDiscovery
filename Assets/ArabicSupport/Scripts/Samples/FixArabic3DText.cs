using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using ArabicSupport;

public class FixArabic3DText : MonoBehaviour {

    public bool showTashkeel = true;
    public bool useHinduNumbers = true;

    // Use this for initialization
    void Start () {
        Text text = gameObject.GetComponent<Text>();

        string fixedText = ArabicFixer.Fix(text.text, showTashkeel, useHinduNumbers);

        text.text = fixedText;

		Debug.Log(fixedText);
    }

}
