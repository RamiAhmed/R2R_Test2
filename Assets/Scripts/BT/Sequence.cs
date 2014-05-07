using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class Sequence : BTObject {

	public List<BTObject> TaskSequence { get; set; }

	private BTObject currentTask = null;


	public Sequence Initialize(List<BTObject> taskSequence) {
		this.TaskSequence = taskSequence;
		this.TaskSequence = this.TaskSequence.OrderByDescending( x => x.Priority ).ToList();

		this.currentTask = this.TaskSequence[0];

		return this;
	}

	public Sequence Initialize(params BTObject[] taskObjects) {
		this.TaskSequence = new List<BTObject>();
		foreach (BTObject obj in taskObjects) {
			this.TaskSequence.Add(obj);
		}

		return Initialize(this.TaskSequence);
	}

	public override void StartObject() {
		if (this.TaskSequence == null || this.TaskSequence.Count <= 0) {
			Debug.LogWarning("Sequence has no Task Sequence list set");
			this.CurrentState = TaskState.TASK_CANCELLED;
		}
		else {
			if (this.CurrentState == TaskState.TASK_WAITING) { 
				this.CurrentState = TaskState.TASK_RUNNING;
				this.currentTask = this.TaskSequence[0];
				this.currentTask.StartObject();

				if (this.Counter > 1 && !this.Looping)
					this.Looping = true;
			}
		}
	}

	public override bool RemoveSelf() {
		if (this.gameObject != null) {
			if (this.Looping && (this.Counter <= 1 || this.counterCount < this.Counter)) {
				//Debug.Log("Looping");
			}
			else {
				while (this.TaskSequence.Count > 0) {
					this.TaskSequence[0].RemoveSelf();
					this.TaskSequence.RemoveAt(0);
				}

				Destroy(this, 0.1f);

				return true;
			}
		}

		return false;
	}

	private void Update() {
		if (this.CurrentState == TaskState.TASK_RUNNING) {
			if (this.currentTask.CurrentState == TaskState.TASK_DONE) {
				this.currentTask = getNextTask();

				if (this.currentTask == null) {
					this.CurrentState = TaskState.TASK_DONE;
				}
				else {
					this.currentTask.StartObject();
				}
			}
			else if (this.currentTask.CurrentState == Task.TaskState.TASK_ABORTED || this.currentTask.CurrentState == Task.TaskState.TASK_CANCELLED) {
				this.CurrentState = this.currentTask.CurrentState;
			}
		}
		else if (this.CurrentState != TaskState.TASK_WAITING) {
			if (!this.bDoneRunning) {
				if (this.Looping) {
					if (this.Counter <= 1 || this.counterCount < this.Counter) {
						this.counterCount++;
						this.CurrentState = TaskState.TASK_WAITING;

						foreach (BTObject task in this.TaskSequence) {
							task.bDoneRunning = false;
							task.CurrentState = TaskState.TASK_WAITING;
						}
					}
					else {
						this.Looping = false;
						this.bDoneRunning = true;
					}
				}
				else 
					this.bDoneRunning = true;
			}
		}
	}

	private BTObject getNextTask() {
		if (this.currentTask == null)
			return null;
		else {
			int index = this.TaskSequence.IndexOf(this.currentTask) + 1;
			return (index < this.TaskSequence.Count) ? this.TaskSequence[index] : null;
		}
	}
}
