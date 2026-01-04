using MelonLoader;


[assembly: MelonInfo(typeof(IncreaseBuyLimit.IncreaseBuyLimitMod), "IncreaseBuyLimit", "1.0.3", "lasersquid", null)]
[assembly: MelonGame("TVGS", "Schedule I")]

namespace IncreaseBuyLimit
{
	public class IncreaseBuyLimitMod : MelonMod
	{
		public HarmonyLib.Harmony harmony = new HarmonyLib.Harmony("com.lasersquid.increasebuylimit");

		public override void OnInitializeMelon()
		{
			LoggerInstance.Msg("Initialized.");
		}
	}
}



// bugs:
//	- buy limit still 999 in supplier meeting dialogue