using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public struct State
{
	public Action OnEnter;
	public Action OnUpdate;
	public Action OnExit;

	public State (Action enter, Action update, Action exit)
	{
		this.OnEnter = enter;
		this.OnUpdate = update;
		this.OnExit = exit;
	}
}

public class StateMachine : MonoBehaviour
{

	public enum UpdateLocal
	{
		Update,
		FixedUpdate,
		LateUpdate
	}

	[HideInInspector]
	public float timeEnterState;

	public Action updateState = delegate() { };
	public string stateName;
	public UpdateLocal updateLocal;
	public Enum previousState;

	private Enum _state;
	private State _currentState;
	private Action inFixed = delegate() { };
	private Action inUpdate = delegate() { };
	private Action inLate = delegate() { };
	private bool _running;

	protected bool firstCallState;
	protected Dictionary<Enum, State> states = new Dictionary<Enum, State> ();

	public Enum currentState {
		get { return _state; }
		set {
			if (!states.ContainsKey (value) || value == _state)
				return;

			previousState = _state;

			_state = value;

			stateName = value.ToString ();

			timeEnterState = Time.time;

			updateState = ExitState;

			if( running )
				CheckUpdateState ();
		}
	}

	public bool running {
		get { return _running; }
		set {
			_running = value;
			if (!value) {
				inUpdate = inFixed = inLate = EmptyMethod;
			} else {
				CheckUpdateState ();
			}
		}
	}

	public void Play( Enum state )
	{
		running = true;
		currentState = state;
	}

	public void Play()
	{
		running = true;
	}

	public void Stop()
	{
		running = false;
	}

	public void AddState( Enum state, Action enter, Action update, Action exit )
	{
		states.Add( state, new State( enter, update, exit ) );
	}

	public void RemoveState( Enum state )
	{
		states.Remove( state );

		if( currentState == state ) 
		{
			foreach( KeyValuePair<Enum, State> pair in states )
			{
				currentState = pair.Key;
				break;
			}
		}
	}

	private void ExitState ()
	{
		if (_currentState.OnExit != null)
			_currentState.OnExit ();

		_currentState = states [_state];
	
		firstCallState = true;

		Enum _previousState = _state;

		if (_currentState.OnEnter != null)
			_currentState.OnEnter ();


		if( _state == _previousState )
		{
			updateState = _currentState.OnUpdate != null ? _currentState.OnUpdate : EmptyMethod;
			updateState += FirstCallUpdate;
		}

		if (!_running)
			inUpdate = inFixed = inLate = EmptyMethod;
		else
			CheckUpdateState ();

	}

	private void FirstCallUpdate()
	{
		firstCallState = false;
		updateState -= FirstCallUpdate;
	}

	private void CheckUpdateState ()
	{
		inUpdate = inFixed = inLate = EmptyMethod;
		switch (updateLocal) {
			case UpdateLocal.Update:
				inUpdate = updateState;
				break;
			case UpdateLocal.FixedUpdate:
				inFixed = updateState;
				break;
			case UpdateLocal.LateUpdate:
				inLate = updateState;
				break;
		}
	}

	public virtual void Update ()
	{
		inUpdate ();
	}

	public virtual void FixedUpdate ()
	{
		inFixed ();
	} 

	public virtual void LateUpdate ()
	{
		inLate ();
	}

	public void EmptyMethod ()
	{
	}


	//utils
	public void PauseWaitingAnimationComplete( string animation )
	{
		StartCoroutine ( StatePauseWaitingAnimationComplete(animation) );
	}

	protected IEnumerator StatePauseWaitingAnimationComplete( string animation )
	{
		running = false;
		yield return StartCoroutine("StateWaitingInitAnimation", animation );
		running = false;
		
		Animator animator = GetComponent<Animator>();

		while( animator.GetCurrentAnimatorStateInfo(0).IsName(animation) && 
		       animator.GetCurrentAnimatorStateInfo(0).normalizedTime <= 0.95f )
			yield return null;

		running = true;
	}

	public void PauseWaitingTime( float time )
	{
		StartCoroutine ( StateWaitingTime(time) );
	}
	
	protected IEnumerator StateWaitingTime ( float time )
	{
		running = false;
		yield return new WaitForSeconds(time);
		running = true;
	}

	public void PauseWaitingInitAnimation( string animation )
	{
		StartCoroutine ( StateWaitingInitAnimation(animation) );
	}

	protected IEnumerator StateWaitingInitAnimation ( string animation )
	{
		running = false;
		float time = Time.time;
		Animator animator = GetComponent<Animator>();
		while ( !animator.GetCurrentAnimatorStateInfo(0).IsName(animation) ) {
			if( Time.time - time > 1 ) break;
			yield return null;
		}
		yield return null;
		running = true;
	}


}
