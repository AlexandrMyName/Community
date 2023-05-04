using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlinkEyes : MonoBehaviour
{
	[SerializeField] Animator animator;
    // Start is called before the first frame update
    void Start()
    {
		StartCoroutine (BlinkAndWait (Random.Range (3, 9)));
    }

	public IEnumerator BlinkAndWait(float waitTime)
	{
		yield return new WaitForSeconds (waitTime);
		animator.SetBool ("blink", true);
		yield return new WaitForSeconds (1);
		animator.SetBool ("blink", false);
		StartCoroutine (BlinkAndWait (Random.Range (3, 9)));
	}
    private void Reset()
    {
        animator = GetComponent<Animator>();
    }
    private void OnValidate()
    {
		animator = GetComponent<Animator>();
    }
}
