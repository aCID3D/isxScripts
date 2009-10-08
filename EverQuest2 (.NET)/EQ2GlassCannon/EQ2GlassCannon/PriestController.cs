﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EQ2.ISXEQ2;

namespace EQ2GlassCannon
{
	public class PriestController : PlayerController
	{
		#region INI settings
		protected bool m_bCastCureCurse = true;
		protected bool m_bPrioritizeCureCurse = true;
		protected bool m_bCastDebuffs = true;
		protected bool m_bCastDamageSpells = true;
		protected string m_strSingleRezCallout = "REZZING << {0} >>";
		protected string m_strGroupRezCallout = "REZZING GROUP << {0} >>";
		protected bool m_bBuffGroupWaterBreathing = true;
		protected bool m_bBuffPhysicalMitigation = true;
		protected StanceType m_eShadowsHealStance = StanceType.Neither;
		protected double m_fHealThresholdRatio = 0.95;
		#endregion

		protected uint m_uiCureAbilityID = 0;
		protected uint m_uiCureCurseAbilityID = 0;
		protected uint m_uiGeneralGroupCureAbilityID = 0;
		protected uint m_uiGeneralSingleDeathSaveAbilityID = 0;
		protected uint m_uiGeneralNonCombatRezAbilityID = 0;
		protected uint m_uiGroupMitigationBuffAbilityID = 0;
		protected uint m_uiGroupWaterBreathingAbilityID = 0;
		protected uint m_uiSpiritOfTheWolfAbilityID = 0;
		protected uint m_uiShadowsOffensiveHealStance = 0;
		protected uint m_uiShadowsDefensiveHealStance = 0;

		/************************************************************************************/
		protected override void TransferINISettings(IniFile ThisFile)
		{
			base.TransferINISettings(ThisFile);

			ThisFile.TransferBool("Priest.CastCureCurse", ref m_bCastCureCurse);
			ThisFile.TransferBool("Priest.PrioritizeCureCurse", ref m_bPrioritizeCureCurse);
			ThisFile.TransferBool("Priest.CastDebuffs", ref m_bCastDebuffs);
			ThisFile.TransferBool("Priest.CastDamageSpells", ref m_bCastDamageSpells);
			ThisFile.TransferString("Priest.SingleRezCallout", ref m_strSingleRezCallout);
			ThisFile.TransferString("Priest.GroupRezCallout", ref m_strGroupRezCallout);
			ThisFile.TransferBool("Priest.BuffGroupWaterBreathing", ref m_bBuffGroupWaterBreathing);
			ThisFile.TransferBool("Priest.BuffPhysicalMitigation", ref m_bBuffPhysicalMitigation);
			ThisFile.TransferEnum<StanceType>("Priest.ShadowsHealStance", ref m_eShadowsHealStance);
			ThisFile.TransferDouble("Priest.HealThresholdRatio", ref m_fHealThresholdRatio);

			return;
		}

		/************************************************************************************/
		protected override void RefreshKnowledgeBook()
		{
			base.RefreshKnowledgeBook();

			m_uiCureAbilityID = SelectHighestAbilityID("Cure");
			m_uiCureCurseAbilityID = SelectHighestAbilityID("Cure Curse");
			m_uiGeneralNonCombatRezAbilityID = SelectHighestAbilityID("Revive");
			m_uiSpiritOfTheWolfAbilityID = SelectHighestAbilityID("Spirit of the Wolf");
			m_uiLoreAndLegendAbilityID = SelectHighestAbilityID("Master's Smite");
			return;
		}

		/************************************************************************************/
		/// <summary>
		/// General purpose cure function for priests.
		/// Evaluates detrimental effects, decides whether single cure or group cure is more appropriate,
		/// and then executes it.
		/// </summary>
		/// <param name="iGroupCureAbilityID"></param>
		/// <param name="bCanGroupTrauma"></param>
		/// <param name="bCanGroupArcane"></param>
		/// <param name="bCanGroupNoxious"></param>
		/// <param name="bCanGroupElemental"></param>
		/// <returns></returns>
		public bool AttemptCures(bool bCanGroupTrauma, bool bCanGroupArcane, bool bCanGroupNoxious, bool bCanGroupElemental)
		{
			if (!m_bCastCures || !MeActor.IsIdle)
				return false;

			bool bGroupCureAvailable = IsAbilityReady(m_uiGeneralGroupCureAbilityID);
			if (!IsAbilityReady(m_uiCureAbilityID) && !bGroupCureAvailable)
				return false;

			string strBestSingleCureCandidate = string.Empty;
			int iMostPotentialCuresAtOnce = 0;
			int iTotalTraumaBearers = 0;
			int iTotalArcaneBearers = 0;
			int iTotalNoxiousBearers = 0;
			int iTotalElementalBearers = 0;

			foreach (VitalStatus ThisStatus in EnumVitalStatuses(m_bCureUngroupedMainTank))
			{
				/// Example: If a group-wide effect is in place,
				/// and then a horrible single-target effect is placed on,
				/// the best cure candidate will have 2 effects as opposed to 1 like everyone else.
				/// This works on Wuoshi for instance.
				int iPotentialCuresAtOnce = 0;

				if (ThisStatus.m_iTrauma > 0)
				{
					iTotalTraumaBearers++;
					iPotentialCuresAtOnce++;
				}
				if (ThisStatus.m_iArcane > 0)
				{
					iTotalArcaneBearers++;
					iPotentialCuresAtOnce++;
				}
				if (ThisStatus.m_iNoxious > 0)
				{
					iTotalNoxiousBearers++;
					iPotentialCuresAtOnce++;
				}
				if (ThisStatus.m_iElemental > 0)
				{
					iTotalElementalBearers++;
					iPotentialCuresAtOnce++;
				}

				if (iPotentialCuresAtOnce > iMostPotentialCuresAtOnce && string.IsNullOrEmpty(strBestSingleCureCandidate))
					strBestSingleCureCandidate = ThisStatus.m_strName;
			}

			/// There's no strict rule on when to cast a group cure; we just fudge it here.
			if (bGroupCureAvailable &&
				(
				(bCanGroupTrauma && iTotalTraumaBearers >= 3) ||
				(bCanGroupArcane && iTotalArcaneBearers >= 3) ||
				(bCanGroupNoxious && iTotalNoxiousBearers >= 3) ||
				(bCanGroupElemental && iTotalElementalBearers >= 3)
				)
				)
			{
				return CastAbilityOnSelf(m_uiGeneralGroupCureAbilityID);
			}
			else if (!string.IsNullOrEmpty(strBestSingleCureCandidate))
			{
				return CastAbility(m_uiCureAbilityID, strBestSingleCureCandidate, true);
			}

			if (m_bCastCureCurse)
			{
			}

			return false;
		}

		/************************************************************************************/
		public bool CheckSpiritOfTheWolf()
		{
			/// This is a distracting spell when over 30 effects are maintained; the bot will never see it after it gets put up.
			/// So unfortunately we hide it for now.
			return false;

			/// We use a simple logic: if I'm not in combat and *I* don't have SOW, then I cast it on myself.
			//return (m_iSpiritOfTheWolfAbilityID != -1) && !Me.IsHated && !IsBeneficialEffectPresent(m_iSpiritOfTheWolfAbilityID) && CastAbilityOnSelf(m_iSpiritOfTheWolfAbilityID);
		}

		/************************************************************************************/
		public bool CheckGroupWaterBreathingBuff()
		{
			if (m_bBuffGroupWaterBreathing)
			{
				/// Only turn it back on if we need to.
				if ((MeActor.IsSwimming || !MeActor.InCombatMode) && CheckToggleBuff(m_uiGroupWaterBreathingAbilityID, true))
					return true;
			}
			else
			{
				if (CheckToggleBuff(m_uiGroupWaterBreathingAbilityID, false))
					return true;
			}

			return false;
		}

		/************************************************************************************/
		public bool CheckShadowsHealStanceBuffs()
		{
			return CheckStanceBuff(m_uiShadowsOffensiveHealStance, m_uiShadowsDefensiveHealStance, m_eShadowsHealStance);
		}

		/************************************************************************************/
		/// <summary>
		/// Arguably this could be exposed to all subclasses,
		/// but it's only useful for classes that potentially never cast offensively.
		/// </summary>
		public bool IsIllusionistSoothingMindActive()
		{
			return IsBeneficialEffectPresent("Soothing Mind");
		}

		/************************************************************************************/
		/// <summary>
		/// Arguably this could be exposed to all subclasses...
		/// </summary>
		public bool IsClericDivineRecoveryActive()
		{
			return IsBeneficialEffectPresent("Divine Recovery");
		}
	}
}