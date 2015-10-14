//delegate void doSomething( int i );
using System;

public class Trigger : ICloneable { 
	public int attachedSlot = -1;
	public object Clone() {
		return this.MemberwiseClone ();
	}
	public const byte FLASH = 0x01;
	public const byte REMOVE = 0x02;
}

public delegate byte createTrigger( IGameplay g, int attachedSlot, int slot );
public delegate bool createTester( IGameInfo g, int attachedSlot, int slot );
public class CreateUnitTrigger : Trigger {
	public readonly createTrigger run;
	public readonly createTester test;
	public CreateUnitTrigger( createTrigger t, createTester test = null ) 
	{ run = t; this.test = test; }
}

public delegate byte destroyTrigger( IGameplay g, int attachedSlot, int slot, IUnitInfo unit );
public delegate bool destroyTester( IGameInfo g, int attachedSlot, int slot, IUnitInfo unit );
public class DestroyUnitTrigger : Trigger {
	public readonly destroyTrigger run;
	public readonly destroyTester test;
	public DestroyUnitTrigger( destroyTrigger t, destroyTester test = null ) 
	{ run = t; this.test = test; }
}

public delegate byte modifyTrigger( IGameplay g, int attachedSlot, int slot, int oldAmount );
public delegate bool modifyTester( IGameInfo g, int attachedSlot, int slot, int oldAmount );
public class ModifyEnergyTrigger : Trigger {
	public readonly modifyTrigger run;
	public readonly modifyTester test;
	public ModifyEnergyTrigger( modifyTrigger t, modifyTester test = null ) 
	{ run = t; this.test = test; }
}

public delegate byte eventTrigger( IGameplay g, int attachedSlot, int slot, EventCard card );
public delegate bool eventTester( IGameInfo g, int attachedSlot, int slot, EventCard card );
public class PlayEventTrigger: Trigger {
	public readonly eventTrigger run;
	public readonly eventTester test;
	public PlayEventTrigger( eventTrigger t, eventTester test = null ) 
	{ run = t; this.test = test; }
}