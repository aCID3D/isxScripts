
;********************************************
function ManualPause()
{
	while ${doPause}
		wait 1
}
;********************************************
function:bool CheckFurious()
{
	while ${mobisfurious}
		{
		if ${Me.HavePet} || ${Me.HaveMinion}
			{
			VGExecute "/pet backoff"
			}
		if ${Me.TargetHealth} > 20
			{
			actionlog "Furious Down Health too High"
			mobisfurious:Set[FALSE]
			wait 10
			return TRUE
			}
		if ${Me.Ability[Auto Attack].Toggled}
			{
			Me.Ability[Auto Attack]:Use
			wait 10
			}
		if ${Me.TargetHealth} == 0 || ${Me.Target.Type.Equal[Corpse]} || !${Me.Target(exists)} || ${Me.Target.IsDead}
			{
			actionlog "Furious Down Mob is Dead/Missing"
			mobisfurious:Set[FALSE]
			wait 10
			return TRUE
			}	
		if ${Me.IsCasting}
			{
			vgexecute /stopcasting
			}
		wait 10
		if ${ClassRole.healer}
			call Healcheck	
		}
	if !${mobisfurious}
		return TRUE	
}
;********************************************
function TurnOffAttackfunct()
{
	If ${doTurnOffAttack} 
	{
		variable iterator Iterator
		TurnOffAttack:GetSettingIterator[Iterator]
		while ( ${Iterator.Key(exists)} )
		{
			if ${Me.TargetBuff[${Iterator.Key}](exists)}
			{
				if ${Me.HavePet} || ${Me.HaveMinion}
					{
					VGExecute "/pet backoff"
					}
				if ${Me.IsCasting}
					{
					vgexecute /stopcasting
					}
				if ${Me.Ability[Auto Attack].Toggled}
					Me.Ability[Auto Attack]:Use
				if ${Me.Ability[{FD}](exists)}
					Me.Ability[${FD}]:Use
				while ${Me.TargetBuff[${Iterator.Key}](exists)}
					{
					if ${Me.HavePet} || ${Me.HaveMinion}
						{
						VGExecute "/pet backoff"
						}
					wait 5
					if ${ClassRole.healer}
						call Healcheck
					}
			}
			Iterator:Next
		}
	}
	call CheckFurious
} 
;********************************************
function TurnOffDuringBuff()
{
	If ${doTurnOffDuringBuff} 
	{
		variable iterator Iterator
		TurnOffDuringBuff:GetSettingIterator[Iterator]
		while ( ${Iterator.Key(exists)} )
		{
			if ${Me.Effect[${Iterator.Key}](exists)}
			{
				if ${Me.IsCasting}
				{
					vgexecute /stopcasting
				}
				if ${Me.Ability[Auto Attack].Toggled}
					Me.Ability[Auto Attack]:Use
				if ${Me.Ability[{FD}](exists)}
					Me.Ability[${FD}]:Use
				while ${Me.Effect[${Iterator.Key}](exists)}
				{
					if ${Me.HavePet}
						{
						VGExecute "/pet backoff"
						}
					wait 5
					if ${ClassRole.healer}
						call Healcheck
				}
			}
			Iterator:Next
		}
	}
	call CheckFurious
} 
;********************************************
function counteringfunct()
{
	If !${Me.TargetCasting.Equal[None]} && ${doCounter}
	{
		actionlog "Mob is Casting ${Me.TargetCasting}"		
		variable iterator Iterator
		Counter:GetSettingIterator[Iterator]
		while ( ${Iterator.Key(exists)} )
		{
			if ${Me.TargetCasting.Find[${Iterator.Key}]}
			{
				if ${Me.IsCasting}
				{
					vgexecute /stopcasting
				}
				while (${VG.InGlobalRecovery} || ${Me.ToPawn.IsStunned} || !${Me.Ability[Torch].IsReady})
				{
				waitframe
				}
				if ${Me.Ability[${CounterSpell1}].IsReady}
				{
					call executeability "${CounterSpell1}" "attack" "Neither"
				}
				elseif ${Me.Ability[${CounterSpell2}].IsReady}
				{
					call executeability "${CounterSpell2}" "attack" "Neither"
				}
			}
			Iterator:Next
		}
	}
}
;********************************************
function clickiesfunct()
{
	if ${doClickies}
	{
		variable iterator Iterator
		Clickies:GetSettingIterator[Iterator]
		while ( ${Iterator.Key(exists)} )
		{
			if ${Me.Inventory[${Iterator.Key}].IsReady}
				{
				waitframe
				Me.Inventory[${Iterator.Key}]:Use
				waitframe
				Me.Inventory[${Iterator.Key}]:Use
				}
		Iterator:Next
		}
	}
}
;********************************************
function dispellfunct()
{
	
	if ${doDispell} && ${Me.TargetBuff} > 0
	{
		variable iterator Iterator
		Dispell:GetSettingIterator[Iterator]
		while ( ${Iterator.Key(exists)} )
		{
			while ${Me.TargetBuff[${Iterator.Key}](exists)}
			{
				while (${VG.InGlobalRecovery} || ${Me.ToPawn.IsStunned} || !${Me.Ability[Torch].IsReady})
				    wait 1
				while !${Me.Ability[${DispellSpell}].IsReady}
					wait 1
				if ${Me.Ability[${DispellSpell}].IsReady}
					call executeability "${DispellSpell}" "attack" "Neither"
			}
			Iterator:Next

		}
		
	}
	
} 
;********************************************
function StancePushfunct()
{
	
	if ${doStancePush} && ${ClassRole.stancepusher}
		{
		variable iterator Iterator
		StancePush:GetSettingIterator[Iterator]
		while ( ${Iterator.Key(exists)} )
			{
				while ${Me.TargetBuff[${Iterator.Key}](exists)}
				{
				while (${VG.InGlobalRecovery} || ${Me.ToPawn.IsStunned} || !${Me.Ability[Torch].IsReady})
				{
				waitframe
				}
				if ${Me.Ability[${PushStanceSpell}].IsReady}
					call executeability "${PushStanceSpell}" "attack" "Neither"
				}
			Iterator:Next
			}
		}
	
}
