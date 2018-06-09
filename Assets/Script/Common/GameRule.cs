using UnityEngine;
using System.Collections;

namespace BKKZ.POW01
{
	public class GameRule
	{
		public const int Default_PvE_Action_Chance = 3;
        /// <summary>
        /// 在移动距离的基础上多驱散两格
        /// 这两个可以看见但不能召唤
        /// </summary>
        public const int Default_PvE_Fog_Lift_Range = 1;
		//表现参数
		//召唤留空时间
		public static float Shoukan_Duration{get{return isShoukanSpeedMult?Multi_Shoukan_Duration:Default_Shoukan_Duration;}}
		public const float Default_Shoukan_Duration = 1f;
		public const float Multi_Shoukan_Duration = 0.5f;
		//剧情移动留空时间
		public static float Moveing_Duration{get{return isDramaMoveingSpdMult?Multi_Moving_Duration:Default_Moving_Duration;}}
		public const float Default_Moving_Duration = 0.5f;
		public const float Multi_Moving_Duration = 0.25f;

		//游戏中的状态切换
		public static bool isShoukanSpeedMult = false;
		public static bool isDramaMoveingSpdMult = false;

        //常用数据
        public const int PlayerChessEventID = 999;
        public static int[] ePlayerIndex = new int[] { 1, 2, 3, 4 };
        public const float FogRandomMax = 0.5f;

		public static bool judgePassable(Chess c,ChessContainer grid){
			switch (grid.terrain_type) {
			case eMapGridType.Unvailable:
			case eMapGridType.Gap:
				return false;
			default:
				return true;
			}
		}
	}
}

