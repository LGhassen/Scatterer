using UnityEngine;
using System.Collections.Generic;

namespace scatterer
{

	/*
	 * The task that creates the tiles. The task calles the producers DoCreateTile function
	 * and the data created is stored in the slot.
	 */
	public class CreateTileTask : Task 
	{

		//The TileProducer that created this task.
		TileProducer m_owner;

		//The level of the tile to create.
		int m_level;

		//The quadtree x coordinate of the tile to create.
		int m_tx;

		//The quadtree y coordinate of the tile to create
		int m_ty;

		//Where the created tile data must be stored
		List<TileStorage.Slot> m_slot;

		public CreateTileTask(TileProducer owner, int level, int tx, int ty, List<TileStorage.Slot> slot)
		{
			m_owner = owner;
			m_level = level;
			m_tx = tx;
			m_ty = ty;
			m_slot = slot;

		}

		public List<TileStorage.Slot> GetSlot() {
			return m_slot;
		}

		public override void Run() 
		{

			if(IsDone()) {
				Debug.Log("Proland::CreateTileTask::Run - task has already been run, task not run");
				return;
			}

			m_owner.DoCreateTile(m_level, m_tx, m_ty, m_slot);

			SetDone(true);
		}

		public override string ToString () {
			return "CreateTileTask, name = " + m_owner.name + " level = " + m_level + " tx = " + m_tx + " ty = " + m_ty;
		}

	}

}















