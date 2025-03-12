
using AEAssist.Extension;
using AEAssist;
using AEAssist.Helper;
using Dalamud.Game.ClientState.Statuses;
using Dalamud.Game.ClientState.Objects.Types;


namespace Frost.Common
{
    public class DamageReductionInfo
    {
        ///<summary>
        ///buff名称
        ///</summary>
        public string Name { get; set; }
        ///<summary>
        ///减伤比例，护盾为恢复力
        ///</summary>
        public int Reduction { get; set; }
        ///<summary>
        ///0单减，1目标减，2团减，3单盾,4群盾
        ///</summary>
        public int Mode { get; set; }

        public DamageReductionInfo(string name, int reduction, int mode)
        {
            Name = name;
            Reduction = reduction;
            Mode = mode;
        }
    }
    internal class DamageReduction
    {
        public static readonly List<DamageReductionInfo> DamageReductionBuffs = new List<DamageReductionInfo>
            {
                //t职能
                new DamageReductionInfo( "铁壁", 20,0),
                new DamageReductionInfo( "雪仇", 10,1),
                new DamageReductionInfo( "减速", 15,1),

                //近战职能
                new DamageReductionInfo( "牵制", 5,1),

                //法系职能
                new DamageReductionInfo( "昏乱", 5,1),

                //骑士
                new DamageReductionInfo( "预警", 30,0),
                new DamageReductionInfo( "极致防御", 40,0),
                new DamageReductionInfo( "极致防御", 1000,3),
                new DamageReductionInfo( "壁垒", 20,0),
                new DamageReductionInfo( "盾阵", 15,0),
                new DamageReductionInfo( "圣盾阵", 15,0),
                new DamageReductionInfo( "干预", 20,0),
                new DamageReductionInfo( "骑士的坚守", 10,0),
                new DamageReductionInfo( "圣光幕帘", 10,2),
                new DamageReductionInfo( "武装戍卫", 15,2),

                //战士
                new DamageReductionInfo( "战栗", 20,0),
                new DamageReductionInfo( "复仇", 30,0),
                new DamageReductionInfo( "戮罪", 40,0),
                new DamageReductionInfo("原初的直觉", 10,0),
                new DamageReductionInfo("原初的血潮", 10,0),
                new DamageReductionInfo( "原初的血气", 10,0),
                new DamageReductionInfo( "原初的武猛", 10,0),
                new DamageReductionInfo( "原初的血烟", 400,3),
                new DamageReductionInfo( "摆脱", 15,2),

                //黑骑
                new DamageReductionInfo( "献奉", 10,0),
                new DamageReductionInfo( "至黑之夜", 25,0),
                new DamageReductionInfo( "暗黑布道", 10,2),


                //枪刃
                new DamageReductionInfo( "石之心", 15,0),
                new DamageReductionInfo( "残暴弹", 200,3),
                new DamageReductionInfo( "刚玉之心", 15,0),
                new DamageReductionInfo( "刚玉之清", 15,0),
                new DamageReductionInfo( "光之心", 10,2),

                //白魔
                new DamageReductionInfo("水流幕", 15,0),
                new DamageReductionInfo( "神祝祷", 500,3),
                new DamageReductionInfo( "节制", 10,2),
                new DamageReductionInfo( "神爱抚", 400,4),

                //占星
                new DamageReductionInfo( "擢升", 10,0),
                new DamageReductionInfo( "天星交错", 400,3),
                new DamageReductionInfo( "命运之轮", 10,2),
                new DamageReductionInfo( "太阳星座", 10,2),
                //todo:中间学派需要维护id
                
                //学者
                new DamageReductionInfo("生命回生法", 10,0),
                new DamageReductionInfo( "鼓舞", 540,3),
                new DamageReductionInfo( "激励", 540,3),
                new DamageReductionInfo( "野战治疗阵", 10,2),
                new DamageReductionInfo( "异想的幻光", 5,2),
                new DamageReductionInfo( "炽天的幻光", 5,2),
                new DamageReductionInfo( "慰藉", 250,4),
                new DamageReductionInfo( "怒涛之计", 10,2),
                //todo:鼓舞需要维护id

                //贤者
                new DamageReductionInfo( "白牛清汁", 10,0),
                new DamageReductionInfo( "均衡诊断", 540,3),
                new DamageReductionInfo( "齐衡诊断", 540,3),
                new DamageReductionInfo( "输血", 300,3),
                new DamageReductionInfo( "均衡预后", 360,4),
                new DamageReductionInfo( "坚角清汁", 10,2),
                new DamageReductionInfo( "整体论", 10,2),
                new DamageReductionInfo( "泛输血", 200,4),

                //赤魔
                new DamageReductionInfo( "抗死", 10,2),

                //远敏
                new DamageReductionInfo( "行吟", 15,2),
                new DamageReductionInfo( "策动", 15,2),
                new DamageReductionInfo( "防守之桑巴", 15,2),

            };

    }
}
