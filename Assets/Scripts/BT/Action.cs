using UnityEngine;
using System.Collections;

public delegate Action.ActionState GetAction();

public class Action  {

	public event GetAction OnAction;

	public enum ActionState {
		ACTION_WAITING,
		ACTION_RUNNING,
		ACTION_DONE,
		ACTION_CANCELLED,
		ACTION_ABORTED
	}

	public ActionState CurrentState = ActionState.ACTION_WAITING;


	public Action(GetAction action) {
		this.AddAction(action);
	}

	public void AddAction(GetAction action) {
		this.OnAction += action;
	}
	public void RemoveAction(GetAction action) {
		this.OnAction -= action;
	}

	public ActionState RunAction() {
		if (this.OnAction != null) {
			this.CurrentState = this.OnAction();
		}
		else {
			Debug.LogWarning("Action has no OnAction set");
			this.CurrentState = ActionState.ACTION_CANCELLED;
		}

		return this.CurrentState;
	}
}
