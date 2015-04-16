using UnityEngine;
using System.Collections.Generic;

public class SetQueue<VALUE> 
{
	
	Dictionary<VALUE, LinkedListNode<KeyValuePair<VALUE,VALUE>>> m_dictionary;
	LinkedList<KeyValuePair<VALUE,VALUE>> m_list;

	public SetQueue() 
	{
		m_dictionary = new Dictionary<VALUE, LinkedListNode<KeyValuePair<VALUE,VALUE>>>();
		m_list = new LinkedList<KeyValuePair<VALUE,VALUE>>();
	}
	
	public SetQueue(IEqualityComparer<VALUE> comparer) 
	{
		m_dictionary = new Dictionary<VALUE, LinkedListNode<KeyValuePair<VALUE,VALUE>>>(comparer);
		m_list = new LinkedList<KeyValuePair<VALUE,VALUE>>();
	}
	
	public bool Contains(VALUE val) {
		return m_dictionary.ContainsKey(val);
	}
	
	public void AddFirst(VALUE val) {
		m_dictionary.Add(val, m_list.AddFirst(new KeyValuePair<VALUE,VALUE>(val,val)));
	}
	
	public void AddLast(VALUE val) {
		m_dictionary.Add(val, m_list.AddLast(new KeyValuePair<VALUE,VALUE>(val,val)));
	}
	
	public int Count() {
		return m_dictionary.Count;
	}
	
	public bool Empty() {
		return (m_dictionary.Count == 0);
	}
	
	public VALUE First() {
		return m_list.First.Value.Value;
	}
	
	public VALUE Last() {
		return m_list.Last.Value.Value;
	}

	public VALUE RemoveFirst()
	{
		LinkedListNode<KeyValuePair<VALUE,VALUE>> node = m_list.First;
		m_list.RemoveFirst();
		m_dictionary.Remove(node.Value.Key);
		return node.Value.Value;
	}
	
	public VALUE RemoveLast()
	{
		LinkedListNode<KeyValuePair<VALUE,VALUE>> node = m_list.Last;
		m_list.RemoveLast();
		m_dictionary.Remove(node.Value.Key);
		return node.Value.Value;
	}
	
	public void Remove(VALUE val)
	{
		LinkedListNode<KeyValuePair<VALUE,VALUE>> node = m_dictionary[val];
		m_dictionary.Remove(val);
		m_list.Remove(node);
	}
	
	public void Clear() 
	{
		m_dictionary.Clear();
		m_list.Clear();
	}
		         

}
