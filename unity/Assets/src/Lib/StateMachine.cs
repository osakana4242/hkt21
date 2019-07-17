using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Osakana4242 {
	public enum StateMachineEventType {
		Enter,
		Update,
	}

	public sealed class StateMachine<T> {
		public struct StateEvent {
			public StateMachineEventType type;
			public T owner;
			public StateMachine<T> sm;
		}

		public delegate StateFunc StateFunc(StateEvent evt);

		StateFunc curState_ = (_evt) => {
			return null;
		};
		StateFunc nextState_;
		public float time;

		public StateMachine(StateFunc firstState) {
			nextState_ = firstState;
		}

		public void Update(T owner) {
			var evt = new StateEvent() {
				owner = owner,
				sm = this,
			};
			if (nextState_ != null) {
				curState_ = nextState_;
				nextState_ = null;
				evt.type = StateMachineEventType.Enter;
				time = 0f;
				curState_(evt);
			}
			evt.type = StateMachineEventType.Update;
			nextState_ = curState_(evt);
			time += Time.deltaTime;
		}
	}
}
