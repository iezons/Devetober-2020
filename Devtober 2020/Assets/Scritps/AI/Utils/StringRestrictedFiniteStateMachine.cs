using System.Collections.Generic;

public class StringRestrictedFiniteStateMachine
{
    private Dictionary<string, List<string>> m_states;

    private string m_currentState;
    private string m_previousState;

    public StringRestrictedFiniteStateMachine( Dictionary<string, List<string>> states, string firstState )
    {
        if (!states.ContainsKey(firstState)) { throw new System.ArgumentException($"The firstState {firstState} is not in the provided states dictionary."); }

        m_states = states;
        m_currentState = firstState;
    }

    public string GetCurrentState()
    {
        return m_currentState;
    }

    public string GetPreviousState()
    {
        return m_previousState;
    }

    public string ChangeState( string newState )
    {
        if (!m_states.ContainsKey(newState)) { throw new System.ArgumentException($"The newState {newState} is not a valid state."); }

        if ( m_states[m_currentState].Contains(newState) )
        {
            m_previousState = m_currentState;
            m_currentState = newState;
        }

        return m_currentState;
    }
}

