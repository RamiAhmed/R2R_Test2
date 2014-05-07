using UnityEngine;
using System.Collections;

public delegate bool GetCondition();

public class Condition {
	
	public event GetCondition OnCondition;

	public bool Result { get; set; }


	public Condition(GetCondition condition) {
		this.AddCondition(condition);
	}

	public void AddCondition(GetCondition condition) {
		this.OnCondition += condition;
	}

	public void RemoveCondition(GetCondition condition) {
		this.OnCondition -= condition;
	}

	public bool GetIsConditionTrue() {
		if (this.OnCondition != null) {
			this.Result = this.OnCondition();
		}

		return this.Result;
	}

}
