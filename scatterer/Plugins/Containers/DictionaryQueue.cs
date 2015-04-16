using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DictionaryQueue<KEY, VALUE>
{
	
	Dictionary<KEY, LinkedListNode<KeyValuePair<KEY,VALUE>>> m_dictionary;
	LinkedList<KeyValuePair<KEY,VALUE>> m_list;

	public DictionaryQueue() 
	{
		m_dictionary = new Dictionary<KEY, LinkedListNode<KeyValuePair<KEY,VALUE>>>();
		m_list = new LinkedList<KeyValuePair<KEY,VALUE>>();
	}

	public DictionaryQueue(IEqualityComparer<KEY> comparer) 
	{
		m_dictionary = new Dictionary<KEY, LinkedListNode<KeyValuePair<KEY,VALUE>>>(comparer);
		m_list = new LinkedList<KeyValuePair<KEY,VALUE>>();
	}

	public bool ContainsKey(KEY key) {
		return m_dictionary.ContainsKey(key);
	}

	public void Replace(KEY key, VALUE val) {
		LinkedListNode<KeyValuePair<KEY,VALUE>> node = m_dictionary[key];
		node.Value = new KeyValuePair<KEY, VALUE>(key, val);
	}

	public void AddFirst(KEY key, VALUE val) {
		m_dictionary.Add(key, m_list.AddFirst(new KeyValuePair<KEY,VALUE>(key,val)));
	}

	public void AddLast(KEY key, VALUE val) {
		m_dictionary.Add(key, m_list.AddLast(new KeyValuePair<KEY,VALUE>(key,val)));
	}

	public int Count() {
		return m_dictionary.Count;
	}

	public bool Empty() {
		return (m_dictionary.Count == 0);
	}

	public VALUE Get(KEY key) {
		LinkedListNode<KeyValuePair<KEY,VALUE>> node = m_dictionary[key];
		return node.Value.Value;
	}

	public KeyValuePair<KEY,VALUE> First() {
		return m_list.First.Value;
	}

	public KeyValuePair<KEY,VALUE> Last() {
		return m_list.Last.Value;
	}

	public VALUE RemoveFirst()
	{
		LinkedListNode<KeyValuePair<KEY,VALUE>> node = m_list.First;
		m_list.RemoveFirst();
		m_dictionary.Remove(node.Value.Key);
		return node.Value.Value;
	}
	
	public VALUE RemoveLast()
	{
		LinkedListNode<KeyValuePair<KEY,VALUE>> node = m_list.Last;
		m_list.RemoveLast();
		m_dictionary.Remove(node.Value.Key);
		return node.Value.Value;
	}

	public VALUE Remove(KEY key)
	{
		LinkedListNode<KeyValuePair<KEY,VALUE>> node = m_dictionary[key];
		m_dictionary.Remove(key);
		m_list.Remove(node);
		return node.Value.Value;
	}
	
	public void Clear() 
	{
		m_dictionary.Clear();
		m_list.Clear();
	}

}



























