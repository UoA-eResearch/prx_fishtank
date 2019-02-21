using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMenu 
{
	void IncrementValue(); 
	void DecrementValue(); 
	/// <summary>
	/// sets the handle position based on the main value for the menu item
	/// </summary>
	void SynchronizeHandleToValue();
}
