using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class Selector : BTObject {

	public List<BTObject> TaskSelectors { get; set; }
	
	private BTObject currentTask = null;


	public Selector Initialize(List<BTObject> taskSelectors) {
		this.TaskSelectors = taskSelectors;
		this.TaskSelectors = this.TaskSelectors.OrderByDescending( x => x.Priority ).ToList();

		this.currentTask = this.TaskSelectors[0];

		return this;
	}

	public Selector Initialize(params BTObject[] taskObjects) {
		this.TaskSelectors = new List<BTObject>();
		foreach (BTObject obj in taskObjects) {
			this.TaskSelectors.Add(obj);
		}
		
		return Initialize(this.TaskSelectors);
	}
	
	public override void StartObject() {
		if (this.TaskSelectors == null || this.TaskSelectors.Count <= 0) {
			Debug.LogWarning("Selector has no Task Selector list set");
			this.CurrentState = TaskState.TASK_CANCELLED;
		}
		else {
			if (this.CurrentState == TaskState.TASK_WAITING) { 
				this.CurrentState = TaskState.TASK_RUNNING;
				this.currentTask = this.TaskSelectors[0];
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
				while (this.TaskSelectors.Count > 0) {
					this.TaskSelectors[0].RemoveSelf();
					this.TaskSelectors.RemoveAt(0);
				}

				Destroy(this, 0.1f);

				return true;
			}
		}

		return false;
	}

	private void Update() {
		if (this.CurrentState == TaskState.TASK_RUNNING) {
			if (this.currentTask.CurrentState == Task.TaskState.TASK_ABORTED || this.currentTask.CurrentState == Task.TaskState.TASK_CANCELLED) {
				this.currentTask = getNextTask();

				if (this.currentTask == null) {
					this.CurrentState = this.currentTask.CurrentState;
				}
				else {
					this.currentTask.StartObject();
				}
			}
			else if (this.currentTask.CurrentState == Task.TaskState.TASK_DONE) {
				this.CurrentState = TaskState.TASK_DONE;
			}
		}
		else if (this.CurrentState != TaskState.TASK_WAITING) {
			if (!this.bDoneRunning) {
				if (this.Looping) {
					if (this.Counter <= 1 || this.counterCount < this.Counter) {
						this.counterCount++;
						this.CurrentState = TaskState.TASK_WAITING;

						foreach (BTObject task in this.TaskSelectors) {
							task.bDoneRunning = false;
							task.CurrentState = TaskState.TASK_WAITING;
						}
					}
					else {
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
			int index = this.TaskSelectors.IndexOf(this.currentTask) + 1;
			return (index < this.TaskSelectors.Count) ? this.TaskSelectors[index] : null;
		}
	}
}
