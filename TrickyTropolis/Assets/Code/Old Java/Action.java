package sequence;

interface Action {
	void run( Gameplay g ); 
}

//This action is intended to be intercepted by a GametoUI to play an animation
class TriggerAction implements Action {
	public final Integer[] slots;
	public TriggerAction( int slot ) { slots = new Integer[] {slot}; }
	public TriggerAction( Integer[] slots ) { this.slots = slots; }
	public void run( Gameplay g ) {}
}

abstract class TargetedAction implements Action {
	public int slotIndex;
	TargetedAction( int slot ) { slotIndex = slot; }
}

class DestroyUnitAction extends TargetedAction {
	DestroyUnitAction(int slot) {super(slot);}

	public void run( Gameplay g ) {
		g.destroyUnit(slotIndex);
	}
}

class BuildUnitAction extends TargetedAction {
	final UnitInfo unit;
	BuildUnitAction( int slot, UnitInfo u ) { super(slot); unit = u; }
	public void run( Gameplay g ) {
		g.buildUnit(slotIndex, unit );
	}
}

class ModifyEnergyAction extends TargetedAction {
	final int amount;
	ModifyEnergyAction( int slot, int a ) { super(slot); amount = a; }
	public void run( Gameplay g) {
		g.modifyEnergy(slotIndex, amount);
	}
}

class AddAttributeAction extends TargetedAction {
	final String attr;
	final int amount;
	AddAttributeAction( int slot, String s, int i ) { super(slot); attr = s; amount = i; }
	public void run( Gameplay g ) {
		g.addAttribute( slotIndex, attr, amount );
	}
}

class RegisterTriggerAction extends TargetedAction {
	final Trigger trigger;
	RegisterTriggerAction( int slot, Trigger t ) { super(slot); trigger = t; }
	public void run( Gameplay g ) {
		g.registerTrigger(trigger);
	}
}

class PowerUpAction implements Action {
	public void run( Gameplay g ) {
		for( int i = 0; i < GameState.MAX_UNITS; ++i )
			g.modifyEnergy(i, 1);
	}
}

//Triggers
/*Each type of Trigger has its own interface.  The method run(...) is invoked
on the trigger whenever that type of event occurs.  The run() method decides
whether or not to execute, then runs itself if appropriate (usually through g.pushAction ).
run() should return false if the trigger didn't go off, true if it did.
*/
abstract class Trigger implements Cloneable {
	private int attachedSlot;
	public int getAttachedSlot() { return attachedSlot; }
	public Trigger makeCopyAtIndex( int slotIndex )  {
		try {
			Trigger t = (Trigger)super.clone();
			t.attachedSlot = slotIndex;
			return t;
		} catch (CloneNotSupportedException e) {
			e.printStackTrace();
			return null;
		}
	}
	
	Trigger( int unit ) { attachedSlot = unit; }
}

abstract class ModifyEnergyTrigger extends Trigger {
	ModifyEnergyTrigger(int unit) { super(unit); }
	abstract public boolean run( Gameplay g, int slot, int oldAmount );
}

abstract class CreateUnitTrigger extends Trigger {
	CreateUnitTrigger(int unit) { super(unit); }
	abstract public boolean run( Gameplay g, int slot );
}

abstract class DestroyUnitTrigger extends Trigger {
	DestroyUnitTrigger(int unit) { super(unit); }
	abstract public boolean run( Gameplay g, int slot );
}

abstract class PlayEventTrigger extends Trigger {
	PlayEventTrigger(int unit) { super(unit); }
	abstract public boolean run( Gameplay g, int slot, EventCard card );
}
