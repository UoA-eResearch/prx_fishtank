using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMenu 
{
	void IncrementValue(); 
	void DecrementValue(); 
	void SynchronizeHandleToValue();
}
