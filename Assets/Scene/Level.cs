using UnityEngine;
using System;
using System.Collections.Generic;
namespace Bridger
{
	public static class Level
	{
		public const float slowMotionTimeScale = 0.075f; //7.5%

		public enum LevelMode
		{
			BUILD,
			PLAY
		}

		public static bool completed = false;
		public static LevelMode mode = LevelMode.BUILD;
		public static List<GoalZone> levelGoals = new List<GoalZone>();
		public static IRevertable currentItem{ get{return undoStack.Peek();} }
		public static Stack<IRevertable> undoStack = new Stack<IRevertable>();
		public static Stack<IRevertable> redoStack = new Stack<IRevertable>();

		static List<IReloadable> levelObjects = new List<IReloadable>();
		static float slowTimeSpeed = 1.25f;

		public static void AddToLevel(IReloadable part)
		{
			levelObjects.Add(part);
			if(part is IRevertable)
			{
				redoStack.Clear();
				undoStack.Push((IRevertable)part);
			}
		}
		
		public static void StartPhysics()
		{
			mode = LevelMode.PLAY;
			foreach(IReloadable part in levelObjects)
			{
				part.StartPhysics();
			}
		}
		public static void Reload()
		{
			completed = false;
			foreach (GoalZone goal in levelGoals)
			{
				goal.completed = false;
			}
			mode = LevelMode.BUILD;
			foreach(IReloadable part in levelObjects)
			{
				part.Reset();
			}
		}
		public static void Clear()
		{
			foreach(IRevertable part in undoStack)
			{
				part.Remove();
				levelObjects.Remove((IReloadable)part);
			}
			undoStack.Clear();
			redoStack.Clear();
		}

		public static void Undo()
		{
			if(undoStack.Count > 0)
			{
				if(undoStack.Peek().Undo()) //Successfully undo
				{
					redoStack.Push(undoStack.Pop());
				}
			}
		}

		public static void Redo()
		{
			if(redoStack.Count > 0)
			{
				if(redoStack.Peek().Redo())//Sucessfully redo
				{
					undoStack.Push(redoStack.Pop());
				}
			}
		}
		public static void Slowmo()
		{
            Time.timeScale = slowMotionTimeScale;
            Time.fixedDeltaTime = 0.02F * Time.timeScale; //by default 30 times pr sec 0.02*1
        }

		public static void UnSlowmo() //TODO put the timecontrol stuff into a lerp thingy.
		{
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02F * Time.timeScale;
        }
        public static void ResetLevel()
        {
            mode = LevelMode.BUILD;
            completed = false;
            levelObjects.Clear();
            undoStack.Clear();
            redoStack.Clear();
        }

    }

	[System.Serializable]
	public struct TransformData
	{
		public Vector3 localPosition, localRotation, localScale;

		public TransformData(Transform t)
		{
			localPosition 	= t.localPosition;
			localRotation 	= t.localRotation.eulerAngles;
			localScale 		= t.localScale;
		}

		public void Reload(Transform t)
		{
			t.localPosition = this.localPosition;
			t.localRotation	= Quaternion.Euler(localRotation);
			t.localScale	= this.localScale;
		}

	}
	/// <summary>
	/// An interface that makes the object able to reset to it's defined original state
	/// </summary>
	public interface IReloadable
	{
		void Reset();
		void StartPhysics();
	}
	/// <summary>
	/// Implements the normal Undo/Redo/Clear functionalities 
	/// </summary>
	public interface IRevertable
	{
		bool Undo();
		bool Redo();
		void Remove();
	}
}

