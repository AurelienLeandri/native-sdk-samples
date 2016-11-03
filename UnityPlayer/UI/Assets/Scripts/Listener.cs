using System;
using System.Collections.Generic;

public interface Listener
{
	void Notify (DZPlayerEvent playerEvent, System.Object eventData);
}
