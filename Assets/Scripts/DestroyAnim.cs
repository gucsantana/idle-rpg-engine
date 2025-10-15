using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAnim : StateMachineBehaviour
{
	// OnStateExit is called when a transition ends and the state machine finishes evaluating this state
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		Destroy(animator.gameObject, stateInfo.length);
	}
}
