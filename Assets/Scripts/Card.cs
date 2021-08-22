using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Card : CardInfo
{

	public KMSelectable selectable;
	public KMHighlightable highlight;

	private MemoryPokerScript mainScript;

	public bool animating { get; private set; }
	public bool faceUp { get; set; }

	private float x;
	private float z;
	
	[SerializeField]
	private TextMesh rankLabel;
	[SerializeField]
	private SpriteRenderer suitLabel;

	void Awake()
	{
		mainScript = transform.parent.GetComponentInParent<MemoryPokerScript>();
		x = transform.localPosition.x;
		z = transform.localPosition.z;
    }
	public void UpdateAppearance()
    {
		rankLabel.text = rank.ToString()[0].ToString();
		suitLabel.sprite = mainScript.sprites[(int)suit];
    }

	public void Flip()
    {
		if (!animating)
			StartCoroutine(FlipAnim());
    }
    public CardInfo Info 
	{
        get
        { return this; }
        set
        {
			rank = value.rank;
			suit = value.suit;
        }
	}

    private IEnumerator FlipAnim()
    {
		mainScript.Audio.PlaySoundAtTransform("Flip", transform);
		animating = true;
		faceUp = !faceUp;
		const float duration = 0.75f;
		float delta = 0;
		Vector3 startRot = new Vector3(faceUp ? -90 : 90,  90, 90);
		Vector3 endRot = new Vector3(faceUp ? 90 : 270,    90, 90);
        while (delta < 1)
        {
			delta += Time.deltaTime / duration;
			transform.localPosition = new Vector3(x, BounceLerp(0.01f, 0.04f, delta), z);
			transform.localEulerAngles = Vector3.Lerp(startRot, endRot, delta);
			yield return null;
        }
		animating = false;
    }

	private float BounceLerp(float start, float end, float t)
    {
		if (t < 0)
			t = 0;
		if (t > 1)
			t = 1;
		if (t <= 0.5)
			return Mathf.Lerp(start, end, t * 2);
		else return Mathf.Lerp(end, start, t * 2 - 1);
    }
	

}
