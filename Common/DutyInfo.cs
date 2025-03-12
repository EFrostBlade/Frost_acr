using AEAssist.MemoryApi;
using AEAssist;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frost.Common
{
    public class DutyInfo
    {
        public bool InDuty { get; }
        public uint Id { get;  }
        public string Name { get; }
        public int MemberNum { get;  }

        public DutyInfo()
        {
            if (Core.Resolve<MemApiCondition>().IsBoundByDuty())
            {
                InDuty = true;
                if(Core.Resolve<MemApiDuty>().DutyInfo == null)
                {
                    Id = Core.Resolve<MemApiZoneInfo>().GetCurrTerrId();
                    Name = "未知副本";
                    MemberNum = 0;
                }
                else
                {
                    Id = Core.Resolve<MemApiZoneInfo>().GetCurrTerrId();
                    Name = Core.Resolve<MemApiDuty>().DutyInfo.Value.Name.ToString();
                    MemberNum = (int)Core.Resolve<MemApiDuty>().DutyInfo.Value.ContentMemberType.Value.Unknown3;
                }
            }
            else
            {
                InDuty = false;
                Id = Core.Resolve<MemApiZoneInfo>().GetCurrTerrId();
                Name = "野外";
                MemberNum = 0;
            }
        }
    }
}
