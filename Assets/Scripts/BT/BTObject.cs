using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class BTObject : MonoBehaviour {

	public enum TaskState {
		TASK_WAITING,
		TASK_RUNNING,
		TASK_DONE,
		TASK_CANCELLED,
		TASK_ABORTED
	};

	public TaskState CurrentState = TaskState.TASK_WAITING;


	public bool Looping = false;
	public int Counter = 0;

	protected int counterCount = 1;

	public float Priority = 0.5f;

	public bool bDoneRunning = false;


	public abstract void StartObject();

	public abstract bool RemoveSelf();

}
